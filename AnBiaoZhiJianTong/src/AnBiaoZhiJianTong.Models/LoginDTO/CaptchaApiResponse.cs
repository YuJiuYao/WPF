using System;
using System.Text.Json.Serialization;

namespace AnBiaoZhiJianTong.Models.LoginDTO
{
    public class CaptchaApiResponse
    {
        [JsonPropertyName("uuid")]
        public string Uuid { get; set; }

        [JsonPropertyName("img")]
        public string ImageBase64 { get; set; }
    }
}
