// ============================================================================
// 推荐放置位置：AnBiaoZhiJianTong.Common/Utilities/LogHelper.cs
// ============================================================================

using System;
using System.Reflection;
using log4net;

namespace AnBiaoZhiJianTong.Common.Utilities
{
    /// <summary>
    /// 日志帮助类 - 自动获取调用方法信息的日志器
    /// 使用 log4net 框架，支持按方法级别的日志记录
    /// </summary>
    public static class LogHelper
    {
        private static string _lastMethodName = "";
        private static ILog _log;

        /// <summary>
        /// 获取当前调用方法的日志器
        /// 日志器名称格式：类型全名@方法名
        /// </summary>
        public static ILog Log
        {
            get
            {
                MethodBase method = new System.Diagnostics.StackTrace().GetFrame(1).GetMethod();
                Type type = method.DeclaringType;
                string full = type.FullName + "@" + method.Name;

                if (_lastMethodName != full)
                {
                    _log = LogManager.GetLogger(full);
                    _lastMethodName = full;
                }

                return _log;
            }
        }

        /// <summary>
        /// 获取指定类型的日志器
        /// </summary>
        /// <param name="type">类型</param>
        /// <returns>日志器实例</returns>
        public static ILog GetLogger(Type type)
        {
            return LogManager.GetLogger(type);
        }

        /// <summary>
        /// 获取指定名称的日志器
        /// </summary>
        /// <param name="name">日志器名称</param>
        /// <returns>日志器实例</returns>
        public static ILog GetLogger(string name)
        {
            return LogManager.GetLogger(name);
        }

        /// <summary>
        /// 获取泛型类型的日志器
        /// </summary>
        /// <typeparam name="T">类型参数</typeparam>
        /// <returns>日志器实例</returns>
        public static ILog GetLogger<T>()
        {
            return LogManager.GetLogger(typeof(T));
        }

        /// <summary>
        /// 记录方法进入日志
        /// </summary>
        /// <param name="parameters">方法参数</param>
        public static void LogMethodEnter(params object[] parameters)
        {
            var method = new System.Diagnostics.StackTrace().GetFrame(1).GetMethod();
            var logger = LogManager.GetLogger(method.DeclaringType.FullName + "@" + method.Name);

            if (logger.IsDebugEnabled)
            {
                string paramStr = parameters != null && parameters.Length > 0
                    ? $" 参数: [{string.Join(", ", parameters)}]"
                    : "";
                logger.Debug($"方法开始执行{paramStr}");
            }
        }

        /// <summary>
        /// 记录方法退出日志
        /// </summary>
        /// <param name="result">返回值</param>
        public static void LogMethodExit(object result = null)
        {
            var method = new System.Diagnostics.StackTrace().GetFrame(1).GetMethod();
            var logger = LogManager.GetLogger(method.DeclaringType.FullName + "@" + method.Name);

            if (logger.IsDebugEnabled)
            {
                string resultStr = result != null ? $" 返回值: {result}" : "";
                logger.Debug($"方法执行完成{resultStr}");
            }
        }

        /// <summary>
        /// 记录异常日志
        /// </summary>
        /// <param name="ex">异常对象</param>
        /// <param name="message">附加消息</param>
        public static void LogException(Exception ex, string message = "")
        {
            var method = new System.Diagnostics.StackTrace().GetFrame(1).GetMethod();
            var logger = LogManager.GetLogger(method.DeclaringType.FullName + "@" + method.Name);

            string fullMessage = string.IsNullOrEmpty(message) ? "发生异常" : message;
            logger.Error(fullMessage, ex);
        }
    }
}
