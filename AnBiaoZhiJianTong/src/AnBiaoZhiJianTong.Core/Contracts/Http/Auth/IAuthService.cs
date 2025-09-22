using System.Threading;
using System.Threading.Tasks;
using AnBiaoZhiJianTong.Models;
using AnBiaoZhiJianTong.Models.LoginDTO;

namespace AnBiaoZhiJianTong.Core.Contracts.Http.Auth
{
    public interface IAuthService
    {
        /// <summary>
        /// 请求短信验证码。
        /// </summary>
        /// <param name="number">手机号。</param>
        /// <param name="ct">取消标记。</param>
        /// <returns>短信接口返回结果。</returns>
        Task<SendSmsApiResponse> RequestSmsCodeAsync(string number, CancellationToken ct = default);

        /// <summary>
        /// 执行验证码登录。
        /// </summary>
        /// <param name="number">手机号。</param>
        /// <param name="captchaCode">短信验证码。</param>
        /// <param name="ct">取消标记。</param>
        /// <returns>登录接口返回结果。</returns>
        Task<ApiResult<LogininApiResponse>> LoginAsync(string number, string captchaCode, CancellationToken ct = default);

        /// <summary>
        /// 当前是否已通过登录验证。
        /// </summary>
        bool IsAuthenticated { get; }

        /// <summary>
        /// 登录后返回的 Token。
        /// </summary>
        string Token { get; }

        /// <summary>
        /// 清空登录态。
        /// </summary>
        void Logout();
    }
}
