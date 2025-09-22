using System.Collections.Generic;
using System.Threading.Tasks;
using AnBiaoZhiJianTong.Models;
using AnBiaoZhiJianTong.Shell.Models;

namespace AnBiaoZhiJianTong.Core.Contracts.SQLite.Mapper
{
    public interface ICheckResultRepository
    {
        List<CheckTypeResult> GetAllCheckTypeResults();
        List<DocumentCheckError> GetAllDocumentCheckErrors();

        Task<List<CheckTypeResult>> GetAllCheckTypeResultsAsync();
        Task<List<DocumentCheckError>> GetAllDocumentCheckErrorsAsync();
    }
}
