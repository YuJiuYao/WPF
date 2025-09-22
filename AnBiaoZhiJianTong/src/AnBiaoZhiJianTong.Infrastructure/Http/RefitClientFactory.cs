using System;
using System.Net.Http;
using System.Text.Json;
using AnBiaoZhiJianTong.Core.Contracts.Configuration;
using AnBiaoZhiJianTong.Core.Contracts.Http;
using AnBiaoZhiJianTong.Core.Contracts.Logging;
using AnBiaoZhiJianTong.Infrastructure.Logging.Extensions;
using Refit;

namespace AnBiaoZhiJianTong.Infrastructure.Http
{
    /// <summary>统一创建 HttpClient 与 Refit 客户端（带签名处理器）。</summary>
    public sealed class RefitClientFactory : IApiClientFactory
    {
        private readonly ILogger _logger;
        private readonly IAppConfiguration _configuration;

        public RefitClientFactory(ILogger logger, IAppConfiguration configuration)
        {
            _logger = logger;
            _configuration = configuration ?? throw new ArgumentNullException(nameof(configuration));
        }

        public HttpClient Create(string baseUrl)
        {
            var url = ResolveBaseUrl(baseUrl);

            // 末端处理器
            var inner = new HttpClientHandler
            {
                UseProxy = false,
                AllowAutoRedirect = false,
                PreAuthenticate = false,
                AutomaticDecompression = System.Net.DecompressionMethods.GZip | System.Net.DecompressionMethods.Deflate
            };

            // 串上签名处理器（关键：给出 inner）
            var pipeline = new RefitZjtAuthHandler(inner);

            // ILogger 注入 
            var logging = new LoggingHandler(pipeline, _logger);

            var http = new HttpClient(logging)
            {
                BaseAddress = new Uri(url),
                Timeout = TimeSpan.FromSeconds(30)
            };

            http.DefaultRequestHeaders.ExpectContinue = false;

            return http;
        }

        public T CreateRefit<T>(string baseUrl)
        {
            var http = Create(baseUrl);
            var settings = new RefitSettings
            {
                ContentSerializer = new SystemTextJsonContentSerializer(new JsonSerializerOptions
                {
                    PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
                    PropertyNameCaseInsensitive = true
                })
            };
            return RestService.For<T>(http, settings);
        }

        private string ResolveBaseUrl(string keyOrUrl)
        {
            if (string.IsNullOrWhiteSpace(keyOrUrl))
            {
                return _configuration.ApiBaseUrl;
            }

            if (Uri.TryCreate(keyOrUrl, UriKind.Absolute, out _))
            {
                return keyOrUrl;
            }

            return _configuration.ApiBaseUrl;
        }

    }
}
