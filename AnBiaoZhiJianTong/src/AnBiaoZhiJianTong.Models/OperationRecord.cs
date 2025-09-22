using SqlSugar;

namespace AnBiaoZhiJianTong.Models
{
    [SugarTable("OperationRecord")]
    public class OperationRecord
    {
        // 主键：INTEGER，自增
        [SugarColumn(IsPrimaryKey = true, IsIdentity = true)]
        public int OperationId { get; set; }

        // 外键到 User(UserId)，Not Null
        [SugarColumn(ColumnDataType = "varchar", Length = 64, IsNullable = false)]
        public string UserId { get; set; }

        /// <summary>
        /// 操作Word路径
        /// </summary>
        [SugarColumn(ColumnDataType = "varchar", Length = 256, IsNullable = true)]
        public string OperationWordName { get; set; }

        /// <summary>
        /// 操作Pdf路径
        /// </summary>
        [SugarColumn(ColumnDataType = "varchar", Length = 256, IsNullable = true)]
        public string OperationPdfName { get; set; }
        /// <summary>
        /// 操作导入规则文件路径
        /// </summary>
        [SugarColumn(ColumnDataType = "varchar", Length = 256, IsNullable = true)]
        public string OperationRuleName { get; set; }
        
        /// <summary>
        /// 导入规则文件生成的规则json路径
        /// </summary>
        [SugarColumn(ColumnDataType = "varchar", Length = 256, IsNullable = true)]
        public string AnBiaoFormatPath { get; set; }

        /// <summary>
        /// 是否检查过（布尔用 INTEGER 0/1）
        /// </summary>
        [SugarColumn(ColumnDataType = "INTEGER", IsNullable = false)]
        public bool IsChecked { get; set; }

        /// <summary>
        /// 是否优化过（布尔用 INTEGER 0/1）
        /// </summary>
        [SugarColumn(ColumnDataType = "INTEGER", IsNullable = false)]
        public bool IsOptimized { get; set; }

        /// <summary>
        /// 优化后的Word路径
        /// </summary>
        [SugarColumn(ColumnDataType = "varchar", Length = 256, IsNullable = true)]
        public string OptimizedWordName { get; set; }

        /// <summary>
        /// 是否导出过（布尔用 INTEGER 0/1）
        /// </summary>
        [SugarColumn(ColumnDataType = "INTEGER", IsNullable = false)]
        public bool IsExported { get; set; }

        // 导航属性（可选）：一对一到 User
        [Navigate(NavigateType.OneToOne, nameof(UserId))]
        public User User { get; set; }
    }
}
