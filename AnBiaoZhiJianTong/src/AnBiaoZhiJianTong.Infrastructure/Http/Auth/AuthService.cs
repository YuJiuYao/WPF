using System;
using System.Threading;
using System.Threading.Tasks;
using AnBiaoZhiJianTong.Core.Contracts.Http;
using AnBiaoZhiJianTong.Core.Contracts.Http.Auth;
using AnBiaoZhiJianTong.Models;
using AnBiaoZhiJianTong.Models.LoginDTO;

namespace AnBiaoZhiJianTong.Infrastructure.Http.Auth
{
    public class AuthService : IAuthService
    {
        private readonly IRefitZjtApi _api;
        private LogininApiResponse _currentLogin;

        public AuthService(IRefitZjtApi api)
        {
            _api = api ?? throw new ArgumentNullException(nameof(api));
        }

        public bool IsAuthenticated => _currentLogin != null;
        public string Token => _currentLogin?.Token ?? string.Empty;

        public async Task<SendSmsApiResponse> RequestSmsCodeAsync(string number, CancellationToken ct = default)
        {
            if (string.IsNullOrWhiteSpace(number))
                throw new ArgumentException("手机号不能为空", nameof(number));

            var request = new GetLoginRequest
            {
                Phone = number.Trim(),
                uiid = string.Empty,
                Captchacode = string.Empty
            };

            return await _api.SendSmsAsync(request, ct).ConfigureAwait(false);
        }

        public async Task<ApiResult<LogininApiResponse>> LoginAsync(string number, string captchaCode, CancellationToken ct = default)
        {
            if (string.IsNullOrWhiteSpace(number))
                throw new ArgumentException("手机号不能为空", nameof(number));
            if (string.IsNullOrWhiteSpace(captchaCode))
                throw new ArgumentException("验证码不能为空", nameof(captchaCode));

            var request = new GetLogMessRequest
            {
                Phone = number.Trim(),
                SmsCode = captchaCode.Trim()
            };

            var result = await _api.RefitLoginAsync(request, ct).ConfigureAwait(false);

            if (result?.Data != null && IsSuccessStatus(result.Code))
                _currentLogin = result.Data;

            return result;
        }

        public void Logout() => _currentLogin = null;

        private static bool IsSuccessStatus(int statusCode) => statusCode == 200 || statusCode == 0;

    }
}
