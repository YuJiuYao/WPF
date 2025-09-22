using SqlSugar;

namespace AnBiaoZhiJianTong.Models
{
    [SugarTable("CheckTypeResult")]
    public class CheckTypeResult
    {
        /// <summary>
        /// 检查点：即DocumentCheckError表中的CheckPointType
        /// </summary>
        [SugarColumn(IsNullable = true)] 
        public string CheckType { get; set; }

        /// <summary>
        /// 检查结果: 1-正确；2-错误；3-人工检查
        /// </summary>
        [SugarColumn(IsNullable = true)] 
        public string CheckResult { get; set; }
    }
}
