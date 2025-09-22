using System;
using System.Text.Json.Serialization;

namespace AnBiaoZhiJianTong.Models.LoginDTO
{
    public class LogininApiResponse
    {
        [JsonPropertyName("user")]
        public User usData { get; set; }
        [JsonPropertyName("token")]
        public string Token { get; set; }
    }
    public class User
    {
        [JsonPropertyName("memberId")]
        public int memberId { get; set; }
        [JsonPropertyName("memberGuid")]
        public string memberGuid { get; set; }
        [JsonPropertyName("memberNo")]
        public string memberNo { get; set; }
        [JsonPropertyName("username")]
        public string username { get; set; }
        [JsonPropertyName("password")]
        public string password { get; set; }
        [JsonPropertyName("level")]
        public string level { get; set; }
        [JsonPropertyName("realName")]
        public string realName { get; set; }
        [JsonPropertyName("mobile")]
        public string mobile { get; set; }
        [JsonPropertyName("mobileAddress")]
        public string mobileAddress { get; set; }
        [JsonPropertyName("company")]
        public string company { get; set; }
        [JsonPropertyName("email")]
        public string email { get; set; }
        [JsonPropertyName("gender")]
        public int gender { get; set; }
        [JsonPropertyName("birthday")]
        public string birthday { get; set; }
        [JsonPropertyName("avatar")]
        public string avatar { get; set; }
        [JsonPropertyName("points")]
        public int points { get; set; }
        [JsonPropertyName("balance")]
        public decimal balance { get; set; }
        [JsonPropertyName("status")]
        public string status { get; set; }
        [JsonPropertyName("createTime")]
        public DateTime createTime { get; set; }
        [JsonPropertyName("createIp")]
        public string createIp { get; set; }
        [JsonPropertyName("createType")]
        public string createType { get; set; }
        [JsonPropertyName("updateTime")]
        public string updateTime { get; set; }
        [JsonPropertyName("comment")]
        public string comment { get; set; }
    }
}

