using System.Text.Json.Serialization;

namespace AnBiaoZhiJianTong.Models.PayDTO
{
    public class PayCheck
    {
        [JsonPropertyName("instanceGuid")]
        public string chekoutId { get; set; }
        [JsonPropertyName("status")]
        public string Status { get; set; }
    }
}
