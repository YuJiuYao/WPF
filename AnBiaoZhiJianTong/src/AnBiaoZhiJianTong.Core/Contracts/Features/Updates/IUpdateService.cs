using System.Threading;
using System.Threading.Tasks;
using AnBiaoZhiJianTong.Models;
using AnBiaoZhiJianTong.Models.UpdateDTO;

namespace AnBiaoZhiJianTong.Core.Contracts.Features.Updates
{
    public interface IUpdateService
    {
        Task<ApiResult<LatestVersionInfo>> CheckAsync(GetLatestVersionRequest req, CancellationToken ct = default);
    }
}
