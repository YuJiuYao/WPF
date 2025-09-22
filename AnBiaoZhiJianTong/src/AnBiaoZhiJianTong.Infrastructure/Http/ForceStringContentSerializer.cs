using System.Net.Http;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;

namespace AnBiaoZhiJianTong.Infrastructure.Http
{
    public class ForceStringContentSerializer : IContentSerializer
    {
        private readonly JsonSerializerOptions _jsonOptions;
        public ForceStringContentSerializer(JsonSerializerOptions jsonOptions)
        {
            _jsonOptions = jsonOptions;
        }
        public async Task<T> DeserializeAsync<T>(HttpContent content)
        {
            var stream = await content.ReadAsStreamAsync();
            return await JsonSerializer.DeserializeAsync<T>(stream, _jsonOptions);
        } 
        public Task<HttpContent> SerializeAsync<T>(T item)
        {
            var json = JsonSerializer.Serialize(item, _jsonOptions);
            // 关键点：强制使用 StringContent
            return Task.FromResult<HttpContent>(
                new StringContent(json, Encoding.UTF8, "application/json")
            );
        }
    }

    public interface IContentSerializer
    {
    }
}
