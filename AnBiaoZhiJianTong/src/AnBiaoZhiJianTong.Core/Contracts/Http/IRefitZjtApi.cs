using System.Threading;
using System.Threading.Tasks;
using AnBiaoZhiJianTong.Models;
using AnBiaoZhiJianTong.Models.LoginDTO;
using AnBiaoZhiJianTong.Models.PayDTO;
using AnBiaoZhiJianTong.Models.UpdateDTO;
using Refit;

namespace AnBiaoZhiJianTong.Core.Contracts.Http
{
    public interface IRefitZjtApi
    {
        [Get("/api/v1/client/version/latest")]
        Task<ApiResult<LatestVersionInfo>> CheckNewVersionAsync([Query] GetLatestVersionRequest query, CancellationToken ct = default);

        [Get("/api/v1/auth/captchaImage")]
        Task<ApiResult<CaptchaApiResponse>> GetCaptchaImageAsync(CancellationToken ct = default);

        [Post("/api/v1/auth/sendCode")]
        Task<SendSmsApiResponse> SendSmsAsync([Body(BodySerializationMethod.Serialized)] GetLoginRequest request, CancellationToken ct = default);

        [Post("/api/v1/auth/login")]
        Task<ApiResult<LogininApiResponse>> RefitLoginAsync([Body(BodySerializationMethod.Serialized)] GetLogMessRequest request, CancellationToken ct = default);

        [Post("/api/v1/instance/verification")]
        Task<ApiResult<PayCheck>> UserPayCheckAsync([Body(BodySerializationMethod.Serialized)] GetUserPayCheckRequest request, CancellationToken ct = default);

        [Post("/api/v1/instance/update")]
        Task<ApiResult<PayCheck>> UpdateAsync([Body(BodySerializationMethod.Serialized)] GetPayOutRequest request, CancellationToken ct = default);
    }



}
