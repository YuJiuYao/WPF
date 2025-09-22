using SqlSugar;

namespace AnBiaoZhiJianTong.Models
{
    [SugarTable("User")]
    public class User
    {
        // 主键：varchar (Not Null)
        [SugarColumn(IsPrimaryKey = true, ColumnDataType = "varchar", Length = 64, IsNullable = false)]
        public string UserId { get; set; }

        // 可空
        [SugarColumn(ColumnDataType = "varchar", Length = 256, IsNullable = true)]
        public string UserName { get; set; }
    }
}
