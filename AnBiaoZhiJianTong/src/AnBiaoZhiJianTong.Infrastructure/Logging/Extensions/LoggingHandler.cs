using System;
using System.Diagnostics;
using System.Linq;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;
using AnBiaoZhiJianTong.Core.Contracts.Logging;

namespace AnBiaoZhiJianTong.Infrastructure.Logging.Extensions
{
    public sealed class LoggingHandler : DelegatingHandler
    {
        private readonly ILogger _log;
        private readonly int _maxBody; // 保护日志体积

        public LoggingHandler(HttpMessageHandler inner, ILogger log, int maxBody = 4096) : base(inner)
        {
            _log = log ?? throw new ArgumentNullException(nameof(log));
            _maxBody = maxBody;
        }

        protected override async Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, CancellationToken ct)
        {
            var sw = Stopwatch.StartNew();
            string reqBody = null;

            try
            {
                // --- 请求日志 ---
                if (request.Content != null)
                {
                    reqBody = await request.Content.ReadAsStringAsync().ConfigureAwait(false);
                    reqBody = Truncate(reqBody, _maxBody);
                }

                var reqHeaders = string.Join("; ", request.Headers
                    .Where(h => !string.Equals(h.Key, "Authorization", StringComparison.OrdinalIgnoreCase)) // 脱敏
                    .Select(h => $"{h.Key}={string.Join(",", h.Value)}"));

                _log.LogInfo($"HTTP → {request.Method} {request.RequestUri} | Headers: {reqHeaders} | Body: {reqBody}");

                // --- 发请求 ---
                var resp = await base.SendAsync(request, ct).ConfigureAwait(false);

                // --- 响应日志 ---
                string respBody = null;
                if (resp.Content != null)
                {
                    respBody = await resp.Content.ReadAsStringAsync().ConfigureAwait(false);
                    respBody = Truncate(respBody, _maxBody);
                }

                var respHeaders = string.Join("; ", resp.Headers.Select(h => $"{h.Key}={string.Join(",", h.Value)}"));

                sw.Stop();
                _log.LogInfo($"HTTP ← {(int)resp.StatusCode} {resp.ReasonPhrase} ({sw.ElapsedMilliseconds} ms) | Headers: {respHeaders} | Body: {respBody}");

                return resp;
            }
            catch (Exception ex)
            {
                sw.Stop();
                _log.LogError($"HTTP ✖ EX ({sw.ElapsedMilliseconds} ms) {request.Method} {request.RequestUri}\n{ex}");
                throw;
            }
        }

        private static string Truncate(string s, int limit)
        {
            if (string.IsNullOrEmpty(s)) return s;
            return s.Length <= limit ? s : s.Substring(0, limit) + $" ...[+{s.Length - limit} chars]";
        }
    }
}
