// SimpleLogger.cs - 简单日志实现
using System;
using System.IO;
using System.Threading;
using AnBiaoZhiJianTong.Core.Contracts.Configuration;
using AnBiaoZhiJianTong.Core.Contracts.Logging;

namespace AnBiaoZhiJianTong.Infrastructure.Logging
{
    /// <summary>
    /// 简单的文件日志实现
    /// </summary>
    public class SimpleLogger : ILogger
    {
        private readonly IAppConfiguration _config;
        private readonly object _lockObject = new object();
        private readonly LogLevel _minLogLevel;
        private string _logDirectory;
        private string _logFilePath;
        private bool _fallbackActivated;

        /// <summary>
        /// 构造函数
        /// </summary>
        /// <param name="config">配置服务</param>
        public SimpleLogger(IAppConfiguration config)
        {
            _config = config;

            // 确保配置已初始化，这样日志路径、日志等级等设置才能正确生效
            _config?.Initialize();

            // 获取配置的最小日志级别
            var minLevel = _config?.GetValue("MinLogLevel", "Info") ?? "Info";
            if (!Enum.TryParse(minLevel, out _minLogLevel))
            {
                _minLogLevel = LogLevel.Info;
            }

            // 设置日志文件路径 - 使用配置的日志路径
            _logDirectory = EnsureLogDirectory();
            _logFilePath = Path.Combine(_logDirectory, $"app_{DateTime.Now:yyyyMMdd}.log");
        }

        /// <summary>
        /// 确保日志目录可用，如失败则自动回退至默认目录。
        /// </summary>
        /// <returns>最终使用的日志目录</returns>
        private string EnsureLogDirectory()
        {
            var configuredPath = _config?.LogPath;
            var resolvedPath = ResolveLogDirectory(configuredPath);

            if (TryEnsureDirectory(resolvedPath))
            {
                return resolvedPath;
            }

            var fallback = GetDefaultLogDirectory();
            TryEnsureDirectory(fallback);
            return fallback;
        }

        /// <summary>
        /// 将日志目录解析为绝对路径。
        /// </summary>
        /// <param name="configuredPath">配置中的目录</param>
        /// <returns>绝对路径</returns>
        private string ResolveLogDirectory(string configuredPath)
        {
            try
            {
                if (string.IsNullOrWhiteSpace(configuredPath))
                {
                    return GetDefaultLogDirectory();
                }

                if (Path.IsPathRooted(configuredPath))
                {
                    return configuredPath;
                }

                var basePath = _config?.AppDataPath;
                if (!string.IsNullOrWhiteSpace(basePath))
                {
                    return Path.Combine(basePath, configuredPath);
                }

                var appDirectory = AppDomain.CurrentDomain.BaseDirectory;
                return string.IsNullOrWhiteSpace(appDirectory)
                    ? Path.GetFullPath(configuredPath)
                    : Path.Combine(appDirectory, configuredPath);
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"解析日志目录失败: {configuredPath}. {ex.Message}");
                return GetDefaultLogDirectory();
            }
        }

        /// <summary>
        /// 确保目录存在。
        /// </summary>
        /// <param name="directory">需要创建的目录</param>
        /// <returns>是否创建成功</returns>
        private bool TryEnsureDirectory(string directory)
        {
            try
            {
                if (!Directory.Exists(directory))
                {
                    Directory.CreateDirectory(directory);
                }
                return true;
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"日志目录创建失败: {directory}. {ex.Message}");
                return false;
            }
        }

        /// <summary>
        /// 获取默认日志目录
        /// </summary>
        /// <returns>默认日志目录</returns>
        private string GetDefaultLogDirectory()
        {
            try
            {
                // 使用应用程序目录下的Logs文件夹
                var appDirectory = AppDomain.CurrentDomain.BaseDirectory;
                return Path.Combine(appDirectory, "Logs");
            }
            catch
            {
                // 备选方案：使用应用数据目录
                var appDataPath = Path.Combine(
                    Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData),
                    "AnBiaoZhiJianTong", "Logs");
                return appDataPath;
            }
        }

        /// <summary>
        /// 记录信息日志
        /// </summary>
        /// <param name="message">消息</param>
        public void LogInfo(string message)
        {
            Log(LogLevel.Info, message);
        }

        /// <summary>
        /// 记录警告日志
        /// </summary>
        /// <param name="message">消息</param>
        public void LogWarning(string message)
        {
            Log(LogLevel.Warning, message);
        }

        /// <summary>
        /// 记录错误日志
        /// </summary>
        /// <param name="message">消息</param>
        public void LogError(string message)
        {
            Log(LogLevel.Error, message);
        }

        /// <summary>
        /// 记录异常日志
        /// </summary>
        /// <param name="exception">异常</param>
        /// <param name="message">附加消息</param>
        public void LogError(Exception exception, string message = null)
        {
            Log(LogLevel.Error, exception, message);
        }

        /// <summary>
        /// 记录调试日志
        /// </summary>
        /// <param name="message">消息</param>
        public void LogDebug(string message)
        {
            Log(LogLevel.Debug, message);
        }

        /// <summary>
        /// 记录跟踪日志
        /// </summary>
        /// <param name="message">消息</param>
        public void LogTrace(string message)
        {
            Log(LogLevel.Trace, message);
        }

        /// <summary>
        /// 记录致命错误日志
        /// </summary>
        /// <param name="message">消息</param>
        public void LogFatal(string message)
        {
            Log(LogLevel.Fatal, message);
        }

        /// <summary>
        /// 记录致命错误异常日志
        /// </summary>
        /// <param name="exception">异常</param>
        /// <param name="message">附加消息</param>
        public void LogFatal(Exception exception, string message = null)
        {
            Log(LogLevel.Fatal, exception, message);
        }

        /// <summary>
        /// 检查是否启用指定级别的日志
        /// </summary>
        /// <param name="level">日志级别</param>
        /// <returns></returns>
        public bool IsEnabled(LogLevel level)
        {
            if (!(_config?.EnableLogging ?? true))
                return false;

            return level >= _minLogLevel;
        }

        /// <summary>
        /// 记录指定级别的日志
        /// </summary>
        /// <param name="level">日志级别</param>
        /// <param name="message">消息</param>
        public void Log(LogLevel level, string message)
        {
            if (!IsEnabled(level) || string.IsNullOrEmpty(message))
                return;

            var logEntry = FormatLogEntry(level, message, null);
            WriteToFile(logEntry);
            WriteToConsole(level, logEntry);
        }

        /// <summary>
        /// 记录指定级别的异常日志
        /// </summary>
        /// <param name="level">日志级别</param>
        /// <param name="exception">异常</param>
        /// <param name="message">附加消息</param>
        public void Log(LogLevel level, Exception exception, string message = null)
        {
            if (!IsEnabled(level))
                return;

            var logMessage = message ?? exception?.Message ?? "Unknown error";
            var logEntry = FormatLogEntry(level, logMessage, exception);
            WriteToFile(logEntry);
            WriteToConsole(level, logEntry);
        }

        /// <summary>
        /// 格式化日志条目
        /// </summary>
        /// <param name="level">日志级别</param>
        /// <param name="message">消息</param>
        /// <param name="exception">异常</param>
        /// <returns></returns>
        private string FormatLogEntry(LogLevel level, string message, Exception exception)
        {
            var timestamp = DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss.fff");
            var threadId = Thread.CurrentThread.ManagedThreadId;
            var levelString = level.ToString().ToUpper().PadRight(7);

            var logEntry = $"[{timestamp}] [{levelString}] [T{threadId:D3}] {message}";

            if (exception != null)
            {
                logEntry += Environment.NewLine + "Exception Details:";
                logEntry += Environment.NewLine + $"  Type: {exception.GetType().FullName}";
                logEntry += Environment.NewLine + $"  Message: {exception.Message}";

                if (!string.IsNullOrEmpty(exception.StackTrace))
                {
                    logEntry += Environment.NewLine + "  StackTrace:";
                    logEntry += Environment.NewLine + exception.StackTrace;
                }

                // 记录内部异常
                var innerException = exception.InnerException;
                var levelInner = 1;
                while (innerException != null && levelInner <= 5) // 最多记录5层内部异常
                {
                    logEntry += Environment.NewLine + $"  InnerException[{levelInner}]:";
                    logEntry += Environment.NewLine + $"    Type: {innerException.GetType().FullName}";
                    logEntry += Environment.NewLine + $"    Message: {innerException.Message}";

                    innerException = innerException.InnerException;
                    levelInner++;
                }
            }

            return logEntry;
        }

        /// <summary>
        /// 写入文件
        /// </summary>
        /// <param name="logEntry">日志条目</param>
        private void WriteToFile(string logEntry)
        {
            try
            {
                lock (_lockObject)
                {
                    File.AppendAllText(_logFilePath, logEntry + Environment.NewLine);
                }
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"日志写入失败: {_logFilePath}. {ex.Message}");
                TryFallbackAndWrite(logEntry);
            }
        }

        /// <summary>
        /// 写入失败时尝试切换到默认目录。
        /// </summary>
        /// <param name="logEntry">日志内容</param>
        private void TryFallbackAndWrite(string logEntry)
        {
            if (_fallbackActivated)
            {
                return;
            }

            var fallbackDirectory = GetDefaultLogDirectory();
            if (!TryEnsureDirectory(fallbackDirectory))
            {
                return;
            }

            lock (_lockObject)
            {
                _fallbackActivated = true;
                _logDirectory = fallbackDirectory;
                _logFilePath = Path.Combine(_logDirectory, $"app_{DateTime.Now:yyyyMMdd}.log");

                try
                {
                    File.AppendAllText(_logFilePath, logEntry + Environment.NewLine);
                }
                catch (Exception writeEx)
                {
                    System.Diagnostics.Debug.WriteLine($"日志写入备用目录仍失败: {_logFilePath}. {writeEx.Message}");
                }
            }
        }

        /// <summary>
        /// 写入控制台
        /// </summary>
        /// <param name="level">日志级别</param>
        /// <param name="logEntry">日志条目</param>
        private void WriteToConsole(LogLevel level, string logEntry)
        {
            try
            {
                // 在调试模式下输出到控制台
                if (System.Diagnostics.Debugger.IsAttached)
                {
                    var originalColor = Console.ForegroundColor;

                    // 根据日志级别设置颜色
                    ConsoleColor color;
                    switch (level)
                    {
                        case LogLevel.Trace:
                            color = ConsoleColor.Gray;
                            break;
                        case LogLevel.Debug:
                            color = ConsoleColor.Cyan;
                            break;
                        case LogLevel.Info:
                            color = ConsoleColor.White;
                            break;
                        case LogLevel.Warning:
                            color = ConsoleColor.Yellow;
                            break;
                        case LogLevel.Error:
                            color = ConsoleColor.Red;
                            break;
                        case LogLevel.Fatal:
                            color = ConsoleColor.Magenta;
                            break;
                        default:
                            color = ConsoleColor.White;
                            break;
                    }

                    Console.ForegroundColor = color;
                    Console.WriteLine(logEntry);
                    Console.ForegroundColor = originalColor;
                }

                // 同时输出到调试窗口
                System.Diagnostics.Debug.WriteLine(logEntry);
            }
            catch
            {
                // 忽略控制台输出错误
            }
        }
    }
}