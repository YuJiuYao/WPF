using System;
using System.IO;
using AnBiaoZhiJianTong.Core.Contracts.Logging;
using AnBiaoZhiJianTong.Core.Contracts.SQLite;
using AnBiaoZhiJianTong.Models;

namespace AnBiaoZhiJianTong.Infrastructure.SQlLite
{
    public sealed class Db3Initializer : IDb3Initializer
    {
        private readonly ILogger _logger;
        private readonly IDb3ContextProvider _provider;

        public Db3Initializer(IDb3ContextProvider provider, ILogger logger)
        {
            _provider = provider;
            _logger = logger;
        }

        public void Initialize()
        {
            // 1) 系统默认缓存目录和 db3 路径
            /*var dbDir = Path.Combine(
                Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData),
                "AnBiaoZhiJianTong", "Cache");
            Directory.CreateDirectory(dbDir);*/
            var dbDir = AppDomain.CurrentDomain.BaseDirectory;

            var dbPath = Path.Combine(dbDir, "anbiao_cache.db3");
            _logger.LogInfo($"dbPath = {dbPath}");
            // 2) 初始化 Provider
            if (!_provider.IsInitialized)
                _provider.Initialize(dbPath, "AnBiaoDb3");

            var db = _provider.GetDb();
            _logger.LogInfo($"db = {db}");

            // 3) 若是首次，会自动创建物理库文件
            db.DbMaintenance.CreateDatabase();

            // 4) CodeFirst 建表（实体）
            db.CodeFirst.InitTables(typeof(User), typeof(OperationRecord));
            _logger.LogInfo($"DB3创建成功");

            // 5) 如需基础数据种子（幂等检查）
            /*if (!db.Queryable<User>().Any())
            {
                db.Insertable(new User
                {
                    UserId = null,
                    UserName = null
                }).ExecuteCommand();
            }*/

        }
    }
}
