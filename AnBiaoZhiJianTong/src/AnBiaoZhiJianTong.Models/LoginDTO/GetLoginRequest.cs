using Refit;
using System.Text.Json.Serialization;

namespace AnBiaoZhiJianTong.Models.LoginDTO
{
    public class GetLoginRequest
    {
        /// <summary>
        /// 用户输入的手机
        /// </summary>
        [JsonPropertyName("mobile")]
        public string Phone { get; set; }
        /// <summary>
        /// 获取的uuid
        /// </summary>
        [JsonPropertyName("uuid")]
        public string uiid { get; set; }
        /// <summary>
        ///获取的验证码结果
        /// </summary>
        [JsonPropertyName("captchacode")]
        public string Captchacode { get; set; }
    }
}
