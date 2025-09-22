using System.Net.Http;

namespace AnBiaoZhiJianTong.Core.Contracts.Http
{
    public interface IApiClientFactory
    {
        /// <summary>
        /// 由实现读取 ISettings，构建 Refit/HttpClient
        /// </summary>
        /// <param name="baseUrlKeyOrAbsolute"></param>
        HttpClient Create(string baseUrlKeyOrAbsolute);
        T CreateRefit<T>(string baseUrlKeyOrAbsolute);
    }
}
