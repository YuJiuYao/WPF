using SqlSugar;

namespace AnBiaoZhiJianTong.Shell.Models
{
    [SugarTable("DocumentCheckError")]
    public class DocumentCheckError
    {
        /// <summary>
        /// 检查项
        /// </summary>
        [SugarColumn(IsNullable = true)] 
        public string CheckPointType { get; set; }

        /// <summary>
        /// 检查类型：A0101、A0102：内容检查 其他：样式检查
        /// </summary>
        [SugarColumn(IsNullable = true)] 
        public string CheckPointTypeCode { get; set; }

        /// <summary>
        /// 检查点名称
        /// </summary>
        [SugarColumn(IsNullable = true)] 
        public string CheckPointName { get; set; }

        /// <summary>
        /// 检查要求
        /// </summary>
        [SugarColumn(IsNullable = true)] 
        public string CheckPointRequirement { get; set; }

        /// <summary>
        ///  文件名称（检查的doc）
        /// </summary>
        [SugarColumn(IsNullable = true)] 
        public string DocumentName { get; set; }

        /// <summary>
        ///  错误点
        /// </summary>
        [SugarColumn(IsNullable = true)] 
        public string ErrorSummary { get; set; }

        /// <summary>
        /// 错误详情
        /// </summary>
        [SugarColumn(IsNullable = true)] 
        public string ErrorDetail { get; set; }

        /// <summary>
        /// 
        /// </summary>
        [SugarColumn(IsNullable = true)] 
        public string CheckedDate { get; set; }

        /// <summary>
        /// 错误章节
        /// </summary>
        [SugarColumn(IsNullable = true)] 
        public string SectionIndex { get; set; }

        /// <summary>
        /// 错误所在页码
        /// </summary>
        [SugarColumn(IsNullable = true)] 
        public string ErrorPageNumber { get; set; }

        /// <summary>
        ///  错误附近段落
        /// </summary>
        [SugarColumn(IsNullable = true)] 
        public string ErrorParagraph { get; set; }

        /// <summary>
        /// 错误附近内容
        /// </summary>
        [SugarColumn(IsNullable = true)] 
        public string ErrorRun { get; set; }

        /// <summary>
        /// 
        /// </summary>
        [SugarColumn(IsNullable = true)] 
        public string CheckResultStatus { get; set; }

        /// <summary>
        /// 
        /// </summary>
        [SugarColumn(IsNullable = true)] 
        public string TenderGuid { get; set; }

        /// <summary>
        /// 
        /// </summary>
        [SugarColumn(IsNullable = true)] 
        public string CompanyGuid { get; set; }
    }
}