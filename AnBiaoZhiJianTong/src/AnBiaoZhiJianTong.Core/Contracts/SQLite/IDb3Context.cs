using SqlSugar;

namespace AnBiaoZhiJianTong.Core.Contracts.SQLite
{
    /// <summary>
    /// SQLite DbContext 抽象接口
    /// </summary>
    public interface IDb3Context
    {
        /// <summary>底层数据库客户端对象（实现层返回具体类型）</summary>
        SqlSugarClient Db { get; }
    }
}
