using System.Collections.Generic;
using System.Threading.Tasks;
using AnBiaoZhiJianTong.Core.Contracts.SQLite;
using AnBiaoZhiJianTong.Models;
using AnBiaoZhiJianTong.Shell.Models;
using SqlSugar;

namespace AnBiaoZhiJianTong.Infrastructure.SQlLite.Mapper
{
    public class CheckResultRepository : Core.Contracts.SQLite.Mapper.ICheckResultRepository
    {
        private readonly IDb3ContextProvider _provider;
        private SqlSugarClient Db => _provider.GetDb();

        public CheckResultRepository(IDb3ContextProvider provider)
        {
            _provider = provider;
        }

        public List<CheckTypeResult> GetAllCheckTypeResults()
            => Db.Queryable<CheckTypeResult>().ToList();

        public List<DocumentCheckError> GetAllDocumentCheckErrors()
            => Db.Queryable<DocumentCheckError>().ToList();

        public Task<List<CheckTypeResult>> GetAllCheckTypeResultsAsync()
            => Db.Queryable<CheckTypeResult>().ToListAsync();

        public Task<List<DocumentCheckError>> GetAllDocumentCheckErrorsAsync()
            => Db.Queryable<DocumentCheckError>().ToListAsync();
    }
}
