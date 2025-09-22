using AnBiaoZhiJianTong.Core.Contracts.SQLite;

namespace AnBiaoZhiJianTong.Infrastructure.SQlLite
{
    public class Db3ContextFactory : IDb3ContextFactory
    {
        public IDb3Context Create(string path, string password = null)
            => new Db3Context(path, password);
    }
}
