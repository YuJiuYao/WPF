// PerformanceLogger.cs - 性能日志记录器
using System;
using System.Diagnostics;
using AnBiaoZhiJianTong.Core.Contracts.Logging;

namespace AnBiaoZhiJianTong.Infrastructure.Services
{
    /// <summary>
    /// 性能日志记录器
    /// </summary>
    public class PerformanceLogger : IDisposable
    {
        private readonly ILogger _logger;
        private readonly string _operationName;
        private readonly Stopwatch _stopwatch;

        /// <summary>
        /// 构造函数
        /// </summary>
        /// <param name="logger">日志服务</param>
        /// <param name="operationName">操作名称</param>
        public PerformanceLogger(ILogger logger, string operationName)
        {
            _logger = logger;
            _operationName = operationName;
            _stopwatch = Stopwatch.StartNew();

            _logger.LogDebug($"开始操作: {_operationName}");
        }

        /// <summary>
        /// 释放资源并记录性能日志
        /// </summary>
        public void Dispose()
        {
            _stopwatch.Stop();

            var elapsedMs = _stopwatch.ElapsedMilliseconds;
            _logger.LogInfo($"完成操作: {_operationName}, 耗时: {elapsedMs}ms");

            // 如果操作耗时过长，记录警告
            if (elapsedMs > 5000)
            {
                _logger.LogWarning($"操作 {_operationName} 耗时过长: {elapsedMs}ms");
            }
        }

        /// <summary>
        /// 创建性能日志记录器
        /// </summary>
        /// <param name="logger">日志服务</param>
        /// <param name="operationName">操作名称</param>
        /// <returns></returns>
        public static PerformanceLogger Create(ILogger logger, string operationName)
        {
            return new PerformanceLogger(logger, operationName);
        }
    }
}