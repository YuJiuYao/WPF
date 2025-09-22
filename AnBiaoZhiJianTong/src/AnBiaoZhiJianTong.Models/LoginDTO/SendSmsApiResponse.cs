using System.Text.Json.Serialization;

namespace AnBiaoZhiJianTong.Models.LoginDTO
{
    public class SendSmsApiResponse
    {
        [JsonPropertyName("msg")]
        public string Message { get; set; }

        [JsonPropertyName("code")]
        public int StatusCode { get; set; }
    }
}
