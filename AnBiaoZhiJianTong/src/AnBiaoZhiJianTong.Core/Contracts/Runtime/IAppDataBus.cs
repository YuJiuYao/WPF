namespace AnBiaoZhiJianTong.Core.Contracts.Runtime
{
    /// <summary>
    /// 通用应用级状态总线（线程安全）
    /// - 支持任意类型 T 的存取
    /// - 支持可选过期（TTL）
    /// - 支持“作用域/会话隔离”（scopeId 可用流程号/文件批次号等）
    /// </summary>
    public interface IAppDataBus
    {
        void Set<T>(string key, T value);
        T Get<T>(string key);
        bool TryGet<T>(string key, out T value);
        void Remove(string key);
        void Clear();
    }
}
