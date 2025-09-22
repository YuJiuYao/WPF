// ILogger.cs - 日志接口
using System;

namespace AnBiaoZhiJianTong.Core.Contracts.Logging
{
    /// <summary>
    /// 日志服务接口
    /// </summary>
    public interface ILogger
    {
        /// <summary>
        /// 记录信息日志
        /// </summary>
        /// <param name="message">消息</param>
        void LogInfo(string message);

        /// <summary>
        /// 记录警告日志
        /// </summary>
        /// <param name="message">消息</param>
        void LogWarning(string message);

        /// <summary>
        /// 记录错误日志
        /// </summary>
        /// <param name="message">消息</param>
        void LogError(string message);

        /// <summary>
        /// 记录异常日志
        /// </summary>
        /// <param name="exception">异常</param>
        /// <param name="message">附加消息</param>
        void LogError(Exception exception, string message = null);

        /// <summary>
        /// 记录调试日志
        /// </summary>
        /// <param name="message">消息</param>
        void LogDebug(string message);

        /// <summary>
        /// 记录跟踪日志
        /// </summary>
        /// <param name="message">消息</param>
        void LogTrace(string message);

        /// <summary>
        /// 记录致命错误日志
        /// </summary>
        /// <param name="message">消息</param>
        void LogFatal(string message);

        /// <summary>
        /// 记录致命错误异常日志
        /// </summary>
        /// <param name="exception">异常</param>
        /// <param name="message">附加消息</param>
        void LogFatal(Exception exception, string message = null);

        /// <summary>
        /// 检查是否启用指定级别的日志
        /// </summary>
        /// <param name="level">日志级别</param>
        /// <returns></returns>
        bool IsEnabled(LogLevel level);

        /// <summary>
        /// 记录指定级别的日志
        /// </summary>
        /// <param name="level">日志级别</param>
        /// <param name="message">消息</param>
        void Log(LogLevel level, string message);

        /// <summary>
        /// 记录指定级别的异常日志
        /// </summary>
        /// <param name="level">日志级别</param>
        /// <param name="exception">异常</param>
        /// <param name="message">附加消息</param>
        void Log(LogLevel level, Exception exception, string message = null);
    }

    /// <summary>
    /// 日志级别枚举
    /// </summary>
    public enum LogLevel
    {
        /// <summary>
        /// 跟踪级别
        /// </summary>
        Trace = 0,

        /// <summary>
        /// 调试级别
        /// </summary>
        Debug = 1,

        /// <summary>
        /// 信息级别
        /// </summary>
        Info = 2,

        /// <summary>
        /// 警告级别
        /// </summary>
        Warning = 3,

        /// <summary>
        /// 错误级别
        /// </summary>
        Error = 4,

        /// <summary>
        /// 致命错误级别
        /// </summary>
        Fatal = 5
    }
}