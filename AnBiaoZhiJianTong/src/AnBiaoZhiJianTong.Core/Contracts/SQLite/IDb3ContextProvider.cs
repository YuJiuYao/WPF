using SqlSugar;

namespace AnBiaoZhiJianTong.Core.Contracts.SQLite
{
    public interface IDb3ContextProvider
    {
        bool IsInitialized { get; }
        string DbPath { get; }
        void Initialize(string path, string password = null);
        SqlSugarClient GetDb();
    }
}
