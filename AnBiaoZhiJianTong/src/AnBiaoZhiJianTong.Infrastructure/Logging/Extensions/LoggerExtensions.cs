// LoggerExtensions.cs - 日志扩展方法
using System;
using System.Runtime.CompilerServices;
using AnBiaoZhiJianTong.Core.Contracts.Logging;

namespace AnBiaoZhiJianTong.Infrastructure.Logging.Extensions
{
    /// <summary>
    /// 日志服务扩展方法
    /// </summary>
    public static class LoggerExtensions
    {
        /// <summary>
        /// 记录方法进入日志
        /// </summary>
        /// <param name="logger">日志服务</param>
        /// <param name="methodName">方法名</param>
        /// <param name="className">类名</param>
        public static void LogMethodEntry(this ILogger logger,
            [CallerMemberName] string methodName = "",
            [CallerFilePath] string className = "")
        {
            var simpleClassName = System.IO.Path.GetFileNameWithoutExtension(className);
            logger.LogTrace($"进入方法: {simpleClassName}.{methodName}");
        }

        /// <summary>
        /// 记录方法退出日志
        /// </summary>
        /// <param name="logger">日志服务</param>
        /// <param name="methodName">方法名</param>
        /// <param name="className">类名</param>
        public static void LogMethodExit(this ILogger logger,
            [CallerMemberName] string methodName = "",
            [CallerFilePath] string className = "")
        {
            var simpleClassName = System.IO.Path.GetFileNameWithoutExtension(className);
            logger.LogTrace($"退出方法: {simpleClassName}.{methodName}");
        }

        /// <summary>
        /// 记录性能日志
        /// </summary>
        /// <param name="logger">日志服务</param>
        /// <param name="operation">操作名称</param>
        /// <param name="elapsedMilliseconds">耗时（毫秒）</param>
        public static void LogPerformance(this ILogger logger, string operation, long elapsedMilliseconds)
        {
            LogLevel level;
            if (elapsedMilliseconds < 100)
            {
                level = LogLevel.Debug;
            }
            else if (elapsedMilliseconds < 1000)
            {
                level = LogLevel.Info;
            }
            else if (elapsedMilliseconds < 5000)
            {
                level = LogLevel.Warning;
            }
            else
            {
                level = LogLevel.Error;
            }

            logger.Log(level, $"性能: {operation} 耗时 {elapsedMilliseconds}ms");
        }

        /// <summary>
        /// 记录对象状态日志
        /// </summary>
        /// <param name="logger">日志服务</param>
        /// <param name="obj">对象</param>
        /// <param name="description">描述</param>
        public static void LogObjectState(this ILogger logger, object obj, string description = null)
        {
            if (obj == null)
            {
                logger.LogDebug($"对象状态 {description}: null");
                return;
            }

            var objectInfo = $"对象状态 {description}: {obj.GetType().Name}";

            // 如果对象重写了ToString方法，则记录其字符串表示
            var toString = obj.ToString();
            if (toString != obj.GetType().FullName)
            {
                objectInfo += $" = {toString}";
            }

            logger.LogDebug(objectInfo);
        }

        /// <summary>
        /// 记录业务操作日志
        /// </summary>
        /// <param name="logger">日志服务</param>
        /// <param name="operation">操作名称</param>
        /// <param name="userId">用户ID</param>
        /// <param name="details">详细信息</param>
        public static void LogBusinessOperation(this ILogger logger, string operation,
            string userId = null, string details = null)
        {
            var message = $"业务操作: {operation}";

            if (!string.IsNullOrEmpty(userId))
            {
                message += $" | 用户: {userId}";
            }

            if (!string.IsNullOrEmpty(details))
            {
                message += $" | 详情: {details}";
            }

            logger.LogInfo(message);
        }
    }
}