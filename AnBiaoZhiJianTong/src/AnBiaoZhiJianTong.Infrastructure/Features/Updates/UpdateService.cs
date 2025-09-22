using System.Threading;
using System.Threading.Tasks;
using AnBiaoZhiJianTong.Core.Contracts.Features.Updates;
using AnBiaoZhiJianTong.Core.Contracts.Http;
using AnBiaoZhiJianTong.Models;
using AnBiaoZhiJianTong.Models.UpdateDTO;

namespace AnBiaoZhiJianTong.Infrastructure.Features.Updates
{
    public sealed class UpdateService : IUpdateService
    {
        private readonly IRefitZjtApi _zjtApi;

        public UpdateService(IRefitZjtApi zjtApi)
        {
            _zjtApi = zjtApi;
        }

        public Task<ApiResult<LatestVersionInfo>> CheckAsync(GetLatestVersionRequest req, CancellationToken ct = default)
            => _zjtApi.CheckNewVersionAsync(req, ct);
    }
}
