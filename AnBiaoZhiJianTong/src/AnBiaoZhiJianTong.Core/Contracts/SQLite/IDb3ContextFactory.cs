namespace AnBiaoZhiJianTong.Core.Contracts.SQLite
{
    public interface IDb3ContextFactory
    {
        IDb3Context Create(string path, string password = null);
    }
}
