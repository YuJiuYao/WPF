using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using AnBiaoZhiJianTong.Common.Utilities;

namespace AnBiaoZhiJianTong.Infrastructure.Http
{
    /// <summary>
    /// 负责为每个请求追加 Zhijiantong-* 请求头与签名。
    /// </summary>
    public class RefitZjtAuthHandler : DelegatingHandler
    {
        private const string AppId = "BYT_AB_000001";

        public RefitZjtAuthHandler(HttpMessageHandler inner) : base(inner) { }

        protected override async Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, CancellationToken cancellationToken)
        {
            // 1) 生成动态参数
            var nonce = Guid.NewGuid().ToString();
            var timestamp = DateTimeOffset.Now.ToUnixTimeMilliseconds().ToString();
            var devicefingerprint = FunctionHelper.GenerateDeviceFingerprint();

            var parameters = new Dictionary<string, string>
            {
                { "Zhijiantong-APPID", AppId },
                { "Zhijiantong-Nonce", nonce },
                { "Zhijiantong-Timestamp", timestamp },
                { "Zhijiantong-Devicefingerprint", devicefingerprint }
            };

            // 2) 生成签名
            var plain = string.Join("&", parameters.OrderBy(p => p.Key).Select(p => $"{p.Key}={p.Value}"));
            var signature = FunctionHelper.Rsa2048Sign(plain);

            // 3) 设置请求头
            var headers = request.Headers;
            headers.Remove("Zhijiantong-APPID");
            headers.TryAddWithoutValidation("Zhijiantong-APPID", AppId);
            headers.TryAddWithoutValidation("Zhijiantong-Signature-Type", "sha256");
            headers.TryAddWithoutValidation("Zhijiantong-Nonce", nonce);
            headers.TryAddWithoutValidation("Zhijiantong-Signature", signature);
            headers.TryAddWithoutValidation("Zhijiantong-Devicefingerprint", devicefingerprint);
            headers.TryAddWithoutValidation("Zhijiantong-Timestamp", timestamp);

            // 统一 JSON Content 的编码与媒体类型
            if (request.Content is not null)
            {
                string json = await request.Content.ReadAsStringAsync().ConfigureAwait(false);
                request.Content = new StringContent(json, Encoding.UTF8, "application/json");
            }
            // 4) 调用后续管道
            return await base.SendAsync(request, cancellationToken).ConfigureAwait(false);
        }
    }
}
