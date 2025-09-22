using Refit;

namespace AnBiaoZhiJianTong.Models.PayDTO
{
    public class GetUserPayCheckRequest
    {
        /// <summary>
        /// 用户校验Id
        /// </summary>
        [AliasAs("memberGuid")]
        public string MemberGuid { get; set; }
        /// <summary>
        /// 
        /// </summary>
        [AliasAs("specialZoneId")]
        public string SpecialZoneId { get; set; }
    }
}
