using Refit;

namespace AnBiaoZhiJianTong.Models.UpdateDTO
{
    public sealed class GetLatestVersionRequest
    {
        /// <summary>
        /// 应用唯一ID
        /// </summary>
        [AliasAs("appCode")]
        public string AppCode { get; set; } = "";
        /// <summary>
        /// 当前客户端版本号
        /// </summary>
        [AliasAs("version")]
        public string Version { get; set; } = "";
    }
}
