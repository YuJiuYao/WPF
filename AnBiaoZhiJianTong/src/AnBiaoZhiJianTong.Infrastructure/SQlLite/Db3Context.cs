using AnBiaoZhiJianTong.Core.Contracts.SQLite;
using SqlSugar;

namespace AnBiaoZhiJianTong.Infrastructure.SQlLite
{
    public class Db3Context : IDb3Context
    {
        public SqlSugarClient Db { get; }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="db3Path"></param>
        /// <param name="password"></param>
        public Db3Context(string db3Path, string password = null)
        {
            var connStr = $"Data Source={db3Path};";
            if (!string.IsNullOrEmpty(password))
            {
                connStr += $"Password={password};";
            }

            Db = new SqlSugarClient(new ConnectionConfig
            {
                ConnectionString = connStr,
                DbType = DbType.Sqlite,
                IsAutoCloseConnection = true,
                InitKeyType = InitKeyType.Attribute
            });

            // 调试 SQL
            Db.Aop.OnLogExecuting = (sql, pars) =>
            {
                System.Diagnostics.Debug.WriteLine(sql);
            };
        }

    }
}
