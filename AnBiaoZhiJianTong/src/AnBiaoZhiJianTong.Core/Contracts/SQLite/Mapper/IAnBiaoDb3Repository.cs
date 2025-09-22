using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using AnBiaoZhiJianTong.Models;

namespace AnBiaoZhiJianTong.Core.Contracts.SQLite.Mapper
{
    public interface IAnBiaoDb3Repository
    {
        #region User表

        /// <summary>
        /// 新增或更新用户（存在则更新，不存在则插入【原子性】）。
        /// </summary>
        /// <param name="user">用户实体（必须包含非空的 <see cref="User.UserId"/>）。</param>
        /// <param name="ct">取消令牌。</param>
        /// <returns>操作是否影响了至少 1 行。</returns>
        /// <exception cref="ArgumentException">当 <paramref name="user"/> 或其 <c>UserId</c> 为空时抛出。</exception>
        Task<bool> UpsertUserAsync(User user, CancellationToken ct = default);

        /// <summary>
        /// 新增或更新用户（存在则更新，不存在则插入【非原子性】）。
        /// </summary>
        /// <param name="user"></param>
        /// <param name="ct"></param>
        /// <returns></returns>
        Task<bool> SaveAbleUserAsync(User user, CancellationToken ct = default);

        /// <summary>
        /// 根据主键 <paramref name="userId"/> 获取用户。
        /// </summary>
        /// <param name="userId">用户主键。</param>
        /// <param name="ct">取消令牌。</param>
        /// <returns>匹配的 <see cref="User"/>；若不存在返回 <c>null</c>。</returns>
        Task<User> GetUserAsync(string userId, CancellationToken ct = default);

        /// <summary>
        /// 批量获取用户列表，可按关键字模糊匹配 <c>UserId</c> 或 <c>UserName</c>。
        /// </summary>
        /// <param name="keyword">可选关键字，模糊匹配 <c>UserId</c> 或 <c>UserName</c>。</param>
        /// <param name="take">最多返回的数量（默认 100）。</param>
        /// <param name="ct">取消令牌。</param>
        /// <returns>按 <c>UserId</c> 升序的用户列表。</returns>
        Task<List<User>> GetUsersAsync(string keyword = null, int take = 100, CancellationToken ct = default);

        /// <summary>
        /// 根据主键删除用户。
        /// </summary>
        /// <param name="userId">用户主键。</param>
        /// <param name="ct">取消令牌。</param>
        /// <returns>受影响的行数。</returns>
        Task<int> DeleteUserAsync(string userId, CancellationToken ct = default);

        #endregion

        #region OperationRecord表

        /// <summary>
        /// 新增一条操作记录。
        /// </summary>
        /// <param name="op">操作记录实体（必须包含非空的 <see cref="OperationRecord.UserId"/>）。</param>
        /// <param name="ct">取消令牌。</param>
        /// <returns>返回数据库生成的自增 <c>OperationId</c>。</returns>
        /// <exception cref="ArgumentException">当 <paramref name="op"/> 为空或 <c>UserId</c> 为空时抛出。</exception>
        Task<int> AddOperationRecordAsync(OperationRecord op, CancellationToken ct = default);

        /// <summary>
        /// 更新一条操作记录（按主键 <c>OperationId</c>）。
        /// </summary>
        /// <param name="op">待更新的操作记录实体（必须包含有效的 <c>OperationId</c>）。</param>
        /// <param name="ct">取消令牌。</param>
        /// <returns>是否至少更新 1 行。</returns>
        /// <exception cref="ArgumentException">当 <paramref name="op"/> 为空或 <c>OperationId</c> 非法时抛出。</exception>
        Task<bool> UpdateOperationRecordAsync(OperationRecord op, CancellationToken ct = default);

        /// <summary>
        /// 新增或更新操作记录（存在则更新，不存在则插入【非原子性】）。
        /// </summary>
        /// <param name="op"></param>
        /// <param name="ct"></param>
        /// <returns></returns>
        Task<bool> SaveAbleOperationRecordAsync(OperationRecord op, CancellationToken ct = default);

        /// <summary>
        /// 根据主键获取操作记录，可选择联表加载 <see cref="User"/>。
        /// </summary>
        /// <param name="operationId">操作记录主键。</param>
        /// <param name="includeUser">是否联表加载关联的用户信息。</param>
        /// <param name="ct">取消令牌。</param>
        /// <returns>匹配的 <see cref="OperationRecord"/>；若不存在返回 <c>null</c>。</returns>
        Task<OperationRecord> GetOperationRecordAsync(int operationId, bool includeUser = false, CancellationToken ct = default);

        /// <summary>
        /// 按条件筛选操作记录（支持按 <c>UserId</c> 与三个布尔状态位过滤）。
        /// </summary>
        /// <param name="userId">可选的用户主键过滤。</param>
        /// <param name="isChecked">过滤已/未检查（<c>null</c> 表示不筛选）。</param>
        /// <param name="isOptimized">过滤已/未优化（<c>null</c> 表示不筛选）。</param>
        /// <param name="isExported">过滤已/未导出（<c>null</c> 表示不筛选）。</param>
        /// <param name="take">最大返回条数（默认 200）。</param>
        /// <param name="includeUser">是否联表加载用户信息。</param>
        /// <param name="ct">取消令牌。</param>
        /// <returns>按 <c>OperationId</c> 倒序的操作记录列表。</returns>
        Task<List<OperationRecord>> ListOperationRecordsAsync(
            string userId = null,
            bool? isChecked = null,
            bool? isOptimized = null,
            bool? isExported = null,
            int take = 200,
            bool includeUser = false,
            CancellationToken ct = default);

        /// <summary>
        /// 根据主键删除操作记录。
        /// </summary>
        /// <param name="operationId">操作记录主键。</param>
        /// <param name="ct">取消令牌。</param>
        /// <returns>受影响的行数。</returns>
        Task<int> DeleteOperationRecordAsync(int operationId, CancellationToken ct = default);

        /// <summary>
        /// 批量更新操作记录的状态位（仅对传入非 <c>null</c> 的字段进行更新）。
        /// </summary>
        /// <param name="operationId">操作记录主键。</param>
        /// <param name="isChecked">若不为 <c>null</c>，更新检查状态。</param>
        /// <param name="isOptimized">若不为 <c>null</c>，更新优化状态。</param>
        /// <param name="isExported">若不为 <c>null</c>，更新导出状态。</param>
        /// <param name="ct">取消令牌。</param>
        /// <returns>受影响的行数。</returns>
        /// <exception cref="ArgumentException">当 <paramref name="operationId"/> 非法时抛出。</exception>
        Task<int> UpdateOperationRecordFlagsAsync(int operationId, bool? isChecked = null, bool? isOptimized = null, bool? isExported = null, CancellationToken ct = default);

        /// <summary>
        /// 获取指定用户最后一次的操作记录
        /// </summary>
        /// <param name="userId"></param>
        /// <param name="ct"></param>
        /// <returns></returns>
        Task<OperationRecord> GetLastOperationRecordAsync(string userId, CancellationToken ct = default);

        #endregion

    }
}
