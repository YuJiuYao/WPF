using System.Text.Json.Serialization;

namespace AnBiaoZhiJianTong.Models.UpdateDTO
{
    public sealed class LatestVersionInfo
    {
        [JsonPropertyName("id")]
        public int Id { get; set; }

        [JsonPropertyName("guid")]
        public string Guid { get; set; } = "";

        [JsonPropertyName("appCode")]
        public string AppCode { get; set; } = "";

        [JsonPropertyName("version")]
        public string Version { get; set; } = "";

        [JsonPropertyName("description")]
        public string Description { get; set; }

        [JsonPropertyName("updateType")]
        public string UpdateType { get; set; } = "";

        [JsonPropertyName("fullPackageUrl")]
        public string FullPackageUrl { get; set; } = "";

        [JsonPropertyName("incrementalPackageUrl")]
        public string IncrementalPackageUrl { get; set; } = "";

        [JsonPropertyName("fileSize")]
        public long? FileSize { get; set; }

        [JsonPropertyName("fullPackageMd5")]
        public string FullPackageMd5 { get; set; }

        [JsonPropertyName("incrementalPackageMd5")]
        public string IncrementalPackageMd5 { get; set; }

        [JsonPropertyName("releaseDate")]
        public string ReleaseDate { get; set; }

        [JsonPropertyName("createTime")]
        public string CreateTime { get; set; }

        [JsonPropertyName("updateTime")]
        public string UpdateTime { get; set; }
    }
}
