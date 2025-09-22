using System.Text.Json.Serialization;

namespace AnBiaoZhiJianTong.Models
{
    public class ApiResult<T>
    {
        /// <summary>
        /// 服务端业务码
        /// </summary>
        [JsonPropertyName("code")] 
        public int Code { get; set; }

        /// <summary>
        /// 失败时的信息
        /// </summary>
        [JsonPropertyName("msg")]
        public string Message { get; set; } = "";

        /// <summary>
        /// 成功时的数据
        /// </summary>
        [JsonPropertyName("data")]
        public T Data { get; set; } = default;
    }
}
