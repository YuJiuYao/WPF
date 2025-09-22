using Refit;


namespace AnBiaoZhiJianTong.Models.PayDTO
{
    public class GetPayOutRequest
    {
        /// <summary>
        /// 用户校验Id
        /// </summary>
        [AliasAs("instanceGuid")]
        public string InstanceGuid { get; set; }
        /// <summary>
        /// 核销状态
        /// </summary>
        [AliasAs("status")]
        public string statu { get; set; }

        /// <summary>
        /// 备注
        /// </summary>
        [AliasAs("remark")]
        public string Remark { get; set; }
    }
}
