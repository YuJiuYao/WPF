using Refit;
using System.Text.Json.Serialization;

namespace AnBiaoZhiJianTong.Models.LoginDTO
{
    public sealed class GetLogMessRequest
    {
        /// <summary>
        /// 用户手机号
        /// </summary>
        [JsonPropertyName("mobile")]
        public string Phone { get; set; } = "";
        /// <summary>
        /// 获取的手机验证码
        /// </summary>
        [JsonPropertyName("code")]
        public string SmsCode { get; set; } = "";
    }
}
