using System;
using System.IO;
using System.Threading;
using AnBiaoZhiJianTong.Core.Contracts.SQLite;
using SqlSugar;

namespace AnBiaoZhiJianTong.Infrastructure.SQlLite
{
    public class Db3ContextProvider : IDb3ContextProvider, IDisposable
    {
        private readonly IDb3ContextFactory _factory;
        private readonly object _lock = new();

        private IDb3Context _context;
        private int _initialized;   // 0 = 未初始化, 1 = 已初始化
        private bool _disposed;

        public Db3ContextProvider(IDb3ContextFactory factory) => _factory = factory;

        public bool IsInitialized => Volatile.Read(ref _initialized) == 1;
        public string DbPath { get; private set; }

        public void Initialize(string path, string password = null)
        {
            if (string.IsNullOrWhiteSpace(path)) throw new ArgumentNullException(nameof(path));
            ThrowIfDisposed();

            // 只允许成功一次
            if (Interlocked.CompareExchange(ref _initialized, 1, 0) == 1)
            {
                // 已初始化过，确保路径一致
                if (!string.Equals(DbPath, path, StringComparison.OrdinalIgnoreCase))
                    throw new InvalidOperationException(
                        $"Db3 已用不同路径初始化：现有 \"{DbPath}\", 新传入 \"{path}\"。若需切换数据库，请先 Dispose 并重新创建 Provider 实例。");
                return;
            }

            lock (_lock)
            {
                Directory.CreateDirectory(Path.GetDirectoryName(Path.GetFullPath(path)) ?? ".");
                DbPath = path;
                _context = _factory.Create(path, password);

                // 可选：SQLite 常用 PRAGMA（首次连接后即可设置）
                var db = _context.Db;
                try
                {
                    // WAL 提升并发，NORMAL 兼顾性能与安全
                    db.Ado.ExecuteCommand("PRAGMA journal_mode=WAL;");
                    db.Ado.ExecuteCommand("PRAGMA synchronous=NORMAL;");
                    db.Ado.ExecuteCommand("PRAGMA foreign_keys=ON;");
                    // 如需限制缓存/内存，可视需要添加：
                    // db.Ado.ExecuteCommand("PRAGMA cache_size = -20000;"); // 约 20MB
                }
                catch { /* 忽略不支持的 PRAGMA */ }

                // AOP：SQL 日志与错误
                db.Aop.OnLogExecuting = (sql, pars) =>
                {
                    // TODO: 接你现有 Logger
                    // logger.LogDebug($"{sql}\n{string.Join(",", pars.Select(p => p.ParameterName+\"=\"+p.Value))}");
                };
                db.Aop.OnError = ex =>
                {
                    // TODO: 接你现有 Logger
                    // logger.LogError(ex, "SqlSugar 执行异常");
                };
            }
        }

        public SqlSugarClient GetDb()
        {
            ThrowIfDisposed();
            if (_context == null)
                throw new InvalidOperationException("Db3 尚未初始化：请在拿到 db3 路径后调用 Initialize(...)。");
            return _context.Db;
        }

        public void Dispose()
        {
            if (_disposed) return;
            lock (_lock)
            {
                if (_context != null)
                {
                    try
                    {
                        // 安全关闭连接池（SqlSugar 的 AutoClose 会处理，但显式更稳妥）
                        _context.Db?.Close();
                    }
                    catch { /* 忽略 */ }
                    finally
                    {
                        _context = null;
                        DbPath = null;
                        Volatile.Write(ref _initialized, 0);
                        _disposed = true;
                    }
                }
            }
        }

        private void ThrowIfDisposed()
        {
            if (_disposed) throw new ObjectDisposedException(nameof(Db3ContextProvider));
        }

    }
}
