using System.Collections.Concurrent;
using AnBiaoZhiJianTong.Core.Contracts.Runtime;

namespace AnBiaoZhiJianTong.Infrastructure.Runtime
{
    public class AppDataBus : IAppDataBus
    {
        private readonly ConcurrentDictionary<string, object> _state = new ConcurrentDictionary<string, object>();

        public void Set<T>(string key, T value) => _state[key] = value;

        public T Get<T>(string key) =>
            _state.TryGetValue(key, out var value) && value is T typed ? typed : default;

        public bool TryGet<T>(string key, out T value)
        {
            if (_state.TryGetValue(key, out var raw) && raw is T typed)
            {
                value = typed;
                return true;
            }
            value = default;
            return false;
        }

        public void Remove(string key) => _state.TryRemove(key, out _);
        public void Clear() => _state.Clear();
    }
}
