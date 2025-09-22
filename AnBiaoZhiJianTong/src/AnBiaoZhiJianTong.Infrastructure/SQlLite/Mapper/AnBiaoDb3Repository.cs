using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using AnBiaoZhiJianTong.Core.Contracts.SQLite;
using AnBiaoZhiJianTong.Core.Contracts.SQLite.Mapper;
using AnBiaoZhiJianTong.Models;
using SqlSugar;

namespace AnBiaoZhiJianTong.Infrastructure.SQlLite.Mapper
{
    public sealed class AnBiaoDb3Repository : IAnBiaoDb3Repository
    {
        private readonly SqlSugarClient _db;

        public AnBiaoDb3Repository(IDb3ContextProvider provider)
        {
            _db = provider.GetDb();
        }

        public async Task<bool> UpsertUserAsync(User user, CancellationToken ct = default)
        {
            if (user == null || string.IsNullOrWhiteSpace(user.UserId))
                throw new ArgumentException("User or UserId is empty.");

            var exists = await _db.Queryable<User>().Where(x => x.UserId == user.UserId).AnyAsync(ct);
            if (exists)
            {
                // 更新除主键外的字段
                var rows = await _db.Updateable(user).IgnoreColumns(it => it.UserId).ExecuteCommandAsync(ct);
                return rows > 0;
            }
            else
            {
                var rows = await _db.Insertable(user).ExecuteCommandAsync(ct);
                return rows > 0;
            }
        }

        public async Task<bool> SaveAbleUserAsync(User user, CancellationToken ct = default)
        {
            if (user == null || string.IsNullOrWhiteSpace(user.UserId))
                throw new ArgumentException("User or UserId is empty.");

            // 按 UserId 决定 Insert 或 Update
            var storage = await _db.Storageable(user)
                .WhereColumns(u => u.UserId)
                .ToStorageAsync(); // 单条用同步即可

            // 需要插入
            if (storage.InsertList.Count > 0)
            {
                var inserted = await storage.AsInsertable.ExecuteCommandAsync(ct);
                return inserted > 0;
            }

            // 需要更新 —— 显式指定要更新的列（至少 1 列）
            if (storage.UpdateList.Count > 0)
            {
                var updated = await storage.AsUpdateable
                    // .IgnoreColumns(u => u.UserId) // 不必写，下面已明确列
                    .UpdateColumns(u => new { u.UserName })
                    .Where(u => u.UserId == user.UserId)  // 明确条件更直观
                    .ExecuteCommandAsync(ct);

                if (updated > 0) return true;

                // 极端并发：更新时该行被删了 → 回退插入一次
                var insertedAgain = await _db.Insertable(user).ExecuteCommandAsync(ct);
                return insertedAgain > 0;
            }

            // 正常不会走到这里；防御：存在但不需要更新时，视为成功
            return true;
        }

        public Task<User> GetUserAsync(string userId, CancellationToken ct = default) => _db.Queryable<User>().Where(x => x.UserId == userId).SingleAsync();


        public Task<List<User>> GetUsersAsync(string keyword = null, int take = 100, CancellationToken ct = default)
        {
            var q = _db.Queryable<User>();
            if (!string.IsNullOrWhiteSpace(keyword))
                q = q.Where(x => x.UserId.Contains(keyword) || x.UserName.Contains(keyword));
            return q.OrderBy(x => x.UserId, OrderByType.Asc).Take(take).ToListAsync(ct);
        }

        public async Task<int> DeleteUserAsync(string userId, CancellationToken ct = default)
        {
            return await _db.Deleteable<User>().Where(x => x.UserId == userId).ExecuteCommandAsync(ct);
        }

        public async Task<int> AddOperationRecordAsync(OperationRecord op, CancellationToken ct = default)
        {
            if (op == null || string.IsNullOrWhiteSpace(op.UserId))
                throw new ArgumentException("Operation or UserId is empty.");

            // 返回自增主键
            return await _db.Insertable(op).ExecuteReturnIdentityAsync(ct);
        }

        public async Task<bool> UpdateOperationRecordAsync(OperationRecord op, CancellationToken ct = default)
        {
            if (op == null || op.OperationId <= 0)
                throw new ArgumentException("OperationId is invalid.");

            var rows = await _db.Updateable(op)
                .IgnoreColumns(it => it.OperationId) // 不更新主键
                .ExecuteCommandAsync(ct);
            return rows > 0;
        }

        public async Task<bool> SaveAbleOperationRecordAsync(OperationRecord op, CancellationToken ct = default)
        {
            if (op == null || string.IsNullOrWhiteSpace(op.UserId))
                throw new ArgumentException("Operation or UserId is empty.");

            // 按 OperationId 决定 Insert 或 Update（OperationId<=0 通常会走插入）
            var storage = await _db.Storageable(op)
                .WhereColumns(o => o.OperationId)
                .ToStorageAsync();

            // 需要插入
            if (storage.InsertList.Count > 0)
            {
                var inserted = await storage.AsInsertable.ExecuteCommandAsync(ct);
                return inserted > 0;
            }

            // 需要更新 —— 忽略自增主键
            if (storage.UpdateList.Count > 0)
            {
                var updated = await storage.AsUpdateable
                    .IgnoreColumns(o => o.OperationId) // 不更新主键
                    .Where(o => o.OperationId == op.OperationId) // 明确条件更直观
                    .ExecuteCommandAsync(ct);

                if (updated > 0) return true;

                // 极端并发：更新时该行被删了 → 回退插入一次
                var insertedAgain = await _db.Insertable(op).ExecuteCommandAsync(ct);
                return insertedAgain > 0;
            }

            // 正常不会到这里；防御：存在但没有需要更新的列时，视为成功
            return true;
        }


        public Task<OperationRecord> GetOperationRecordAsync(int operationId, bool includeUser = false, CancellationToken ct = default)
        {
            var q = _db.Queryable<OperationRecord>().Where(x => x.OperationId == operationId);
            if (includeUser) q = q.Includes(o => o.User);
            return q.SingleAsync();
        }

        public Task<List<OperationRecord>> ListOperationRecordsAsync(string userId = null, bool? isChecked = null, bool? isOptimized = null,
            bool? isExported = null, int take = 200, bool includeUser = false, CancellationToken ct = default)
        {
            var q = _db.Queryable<OperationRecord>();

            if (!string.IsNullOrWhiteSpace(userId)) q = q.Where(x => x.UserId == userId);
            if (isChecked.HasValue) q = q.Where(x => x.IsChecked == isChecked);
            if (isOptimized.HasValue) q = q.Where(x => x.IsOptimized == isOptimized);
            if (isExported.HasValue) q = q.Where(x => x.IsExported == isExported);

            if (includeUser) q = q.Includes(o => o.User);

            return q.OrderBy(x => x.OperationId, OrderByType.Desc)
                .Take(take)
                .ToListAsync(ct);
        }

        public Task<int> DeleteOperationRecordAsync(int operationId, CancellationToken ct = default)
            => _db.Deleteable<OperationRecord>().Where(x => x.OperationId == operationId).ExecuteCommandAsync(ct);

        public Task<int> UpdateOperationRecordFlagsAsync(int operationId, bool? isChecked = null, bool? isOptimized = null,
            bool? isExported = null, CancellationToken ct = default)
        {
            if (operationId <= 0) throw new ArgumentException("OperationId is invalid.");

            var updater = _db.Updateable<OperationRecord>().Where(x => x.OperationId == operationId);

            if (isChecked.HasValue) updater = updater.SetColumns(x => x.IsChecked == isChecked.Value);
            if (isOptimized.HasValue) updater = updater.SetColumns(x => x.IsOptimized == isOptimized.Value);
            if (isExported.HasValue) updater = updater.SetColumns(x => x.IsExported == isExported.Value);

            return updater.ExecuteCommandAsync(ct);
        }


        public Task<OperationRecord> GetLastOperationRecordAsync(string userId, CancellationToken ct = default)
        {
            var q = _db.Queryable<OperationRecord>().Where(x => x.UserId == userId);
            return q.OrderBy(x => x.OperationId, OrderByType.Desc).FirstAsync(ct);
        }
    }
}
