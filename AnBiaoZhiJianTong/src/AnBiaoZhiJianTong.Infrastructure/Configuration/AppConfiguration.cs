// AppConfiguration.cs - 应用配置实现
using System;
using System.Collections.Generic;
using System.Configuration;
using System.IO;
using System.Linq;
using AnBiaoZhiJianTong.Core.Contracts.Configuration;
using Newtonsoft.Json;

namespace AnBiaoZhiJianTong.Infrastructure.Configuration
{
    /// <summary>
    /// 应用配置实现
    /// </summary>
    public class AppConfiguration : IAppConfiguration
    {
        #region Private Fields

        private System.Configuration.Configuration _appConfig;
        private readonly Dictionary<string, object> _userSettings;
        private readonly string _userConfigPath;
        private readonly object _lockObject = new object();

        #endregion

        #region Constructor

        /// <summary>
        /// 构造函数
        /// </summary>
        public AppConfiguration()
        {
            _userSettings = new Dictionary<string, object>();

            // 设置用户配置文件路径
            var appDataPath = Path.Combine(
                Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData),
                "AnBiaoZhiJianTong");

            if (!Directory.Exists(appDataPath))
            {
                Directory.CreateDirectory(appDataPath);
            }

            _userConfigPath = Path.Combine(appDataPath, "user-settings.json");
        }

        #endregion

        #region 基础属性

        public string ApiBaseUrl => GetValue("ApiBaseUrl", "https://www.tlgb.cn/");

        public string AppDataPath => GetValue("AppDataPath",
            Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData), "AnBiaoZhiJianTong"));

        public string LogPath => GetValue("LogPath", Path.Combine(AppDataPath, "Logs"));

        public long MaxFileSize => GetValue("MaxFileSize", 52428800L); // 50MB

        public string[] SupportedFileExtensions => GetValue("SupportedFileExtensions", ".doc,.docx,.pdf,.txt")
            .Split([','], StringSplitOptions.RemoveEmptyEntries)
            .Select(ext => ext.Trim())
            .ToArray();

        public TimeSpan RequestTimeout => TimeSpan.FromSeconds(GetValue("RequestTimeoutSeconds", 30));

        public bool EnableLogging => GetValue("EnableLogging", true);

        public string MinLogLevel => GetValue("MinLogLevel", "Info");

        public bool IsDebugMode => GetValue("IsDebugMode", false);

        public string AppVersion => GetValue("AppVersion", GetApplicationVersion());

        public string AppName => GetValue("AppName", "暗标质检通");

        public string CompanyName => GetValue("CompanyName", "中星辰宇(杭州)数字科技有限公司");

        #endregion

        #region UI设置

        public string Theme => GetValue("Theme", "Light");

        public string Language => GetValue("Language", "zh-CN");

        public bool ShowSearchBox => GetValue("ShowSearchBox", false);

        public bool MinimizeToTray => GetValue("MinimizeToTray", false);

        public bool EnableAutoUpdate => GetValue("EnableAutoUpdate", true);

        public WindowSettings WindowSettings => GetValue("WindowSettings", new WindowSettings());

        #endregion

        #region 业务设置

        public int[] DefaultCheckRuleIds => GetValue("DefaultCheckRuleIds", "1,2,3,4,5,6,7,8")
            .Split(new[] { ',' }, StringSplitOptions.RemoveEmptyEntries)
            .Select(id => int.TryParse(id.Trim(), out var result) ? result : 0)
            .Where(id => id > 0)
            .ToArray();

        public int AutoSaveInterval => GetValue("AutoSaveInterval", 5);

        public int MaxHistoryCount => GetValue("MaxHistoryCount", 100);

        public bool EnableFileCache => GetValue("EnableFileCache", true);

        public int CacheExpirationHours => GetValue("CacheExpirationHours", 24);

        #endregion

        #region Events

        public event EventHandler<ConfigurationChangedEventArgs> ConfigurationChanged;

        #endregion

        #region Public Methods

        /// <summary>
        /// 初始化配置
        /// </summary>
        public void Initialize()
        {
            try
            {
                // 加载应用程序配置
                LoadAppConfig();

                // 加载用户配置
                LoadUserConfig();

                // 确保必要的目录存在
                EnsureDirectoriesExist();
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"配置初始化失败: {ex.Message}");
                // 使用默认配置
            }
        }

        /// <summary>
        /// 获取配置值
        /// </summary>
        /// <typeparam name="T">值类型</typeparam>
        /// <param name="key">配置键</param>
        /// <param name="defaultValue">默认值</param>
        /// <returns>配置值</returns>
        public T GetValue<T>(string key, T defaultValue = default)
        {
            lock (_lockObject)
            {
                try
                {
                    // 首先检查用户设置
                    if (_userSettings.TryGetValue(key, out var userValue))
                    {
                        return ConvertValue(userValue, defaultValue);
                    }

                    // 然后检查应用程序配置
                    var appSetting = _appConfig?.AppSettings?.Settings[key]?.Value;
                    if (!string.IsNullOrEmpty(appSetting))
                    {
                        return ConvertValue<T>(appSetting, defaultValue);
                    }

                    return defaultValue;
                }
                catch
                {
                    return defaultValue;
                }
            }
        }

        /// <summary>
        /// 设置配置值
        /// </summary>
        /// <typeparam name="T">值类型</typeparam>
        /// <param name="key">配置键</param>
        /// <param name="value">配置值</param>
        public void SetValue<T>(string key, T value)
        {
            lock (_lockObject)
            {
                var oldValue = _userSettings.TryGetValue(key, out var old) ? old : default(T);
                _userSettings[key] = value;

                // 触发配置变更事件
                OnConfigurationChanged(key, oldValue, value);
            }
        }

        /// <summary>
        /// 检查配置键是否存在
        /// </summary>
        /// <param name="key">配置键</param>
        /// <returns>是否存在</returns>
        public bool ContainsKey(string key)
        {
            lock (_lockObject)
            {
                return _userSettings.ContainsKey(key) ||
                       _appConfig?.AppSettings?.Settings[key] != null;
            }
        }

        /// <summary>
        /// 移除配置项
        /// </summary>
        /// <param name="key">配置键</param>
        /// <returns>是否成功移除</returns>
        public bool RemoveKey(string key)
        {
            lock (_lockObject)
            {
                return _userSettings.Remove(key);
            }
        }

        /// <summary>
        /// 获取所有配置键
        /// </summary>
        /// <returns>配置键列表</returns>
        public IEnumerable<string> GetAllKeys()
        {
            lock (_lockObject)
            {
                var userKeys = _userSettings.Keys;
                var appKeys = _appConfig?.AppSettings?.Settings?.AllKeys ?? new string[0];
                return userKeys.Union(appKeys).Distinct();
            }
        }

        /// <summary>
        /// 保存配置
        /// </summary>
        public void Save()
        {
            lock (_lockObject)
            {
                try
                {
                    SaveUserConfig();
                }
                catch (Exception ex)
                {
                    System.Diagnostics.Debug.WriteLine($"保存配置失败: {ex.Message}");
                }
            }
        }

        /// <summary>
        /// 重新加载配置
        /// </summary>
        public void Reload()
        {
            lock (_lockObject)
            {
                try
                {
                    LoadAppConfig();
                    LoadUserConfig();
                }
                catch (Exception ex)
                {
                    System.Diagnostics.Debug.WriteLine($"重新加载配置失败: {ex.Message}");
                }
            }
        }

        /// <summary>
        /// 重置为默认配置
        /// </summary>
        public void ResetToDefaults()
        {
            lock (_lockObject)
            {
                _userSettings.Clear();
                Save();
            }
        }

        /// <summary>
        /// 导出配置到文件
        /// </summary>
        /// <param name="filePath">文件路径</param>
        /// <returns>是否成功</returns>
        public bool ExportToFile(string filePath)
        {
            try
            {
                lock (_lockObject)
                {
                    var json = JsonConvert.SerializeObject(_userSettings, Formatting.Indented);
                    File.WriteAllText(filePath, json);
                    return true;
                }
            }
            catch
            {
                return false;
            }
        }

        /// <summary>
        /// 从文件导入配置
        /// </summary>
        /// <param name="filePath">文件路径</param>
        /// <returns>是否成功</returns>
        public bool ImportFromFile(string filePath)
        {
            try
            {
                if (!File.Exists(filePath))
                    return false;

                lock (_lockObject)
                {
                    var json = File.ReadAllText(filePath);
                    var settings = JsonConvert.DeserializeObject<Dictionary<string, object>>(json);

                    if (settings != null)
                    {
                        foreach (var kvp in settings)
                        {
                            _userSettings[kvp.Key] = kvp.Value;
                        }

                        Save();
                        return true;
                    }
                }
            }
            catch
            {
                // 忽略导入错误
            }

            return false;
        }

        #endregion

        #region Private Methods

        /// <summary>
        /// 加载应用程序配置
        /// </summary>
        private void LoadAppConfig()
        {
            try
            {
                _appConfig = ConfigurationManager.OpenExeConfiguration(ConfigurationUserLevel.None);
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"加载应用程序配置失败: {ex.Message}");
            }
        }

        /// <summary>
        /// 加载用户配置
        /// </summary>
        private void LoadUserConfig()
        {
            try
            {
                if (File.Exists(_userConfigPath))
                {
                    var json = File.ReadAllText(_userConfigPath);
                    var settings = JsonConvert.DeserializeObject<Dictionary<string, object>>(json);

                    if (settings != null)
                    {
                        _userSettings.Clear();
                        foreach (var kvp in settings)
                        {
                            _userSettings[kvp.Key] = kvp.Value;
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"加载用户配置失败: {ex.Message}");
            }
        }

        /// <summary>
        /// 保存用户配置
        /// </summary>
        private void SaveUserConfig()
        {
            try
            {
                var json = JsonConvert.SerializeObject(_userSettings, Formatting.Indented);
                File.WriteAllText(_userConfigPath, json);
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"保存用户配置失败: {ex.Message}");
            }
        }

        /// <summary>
        /// 确保必要的目录存在
        /// </summary>
        private void EnsureDirectoriesExist()
        {
            try
            {
                // 确保应用数据目录存在
                if (!Directory.Exists(AppDataPath))
                {
                    Directory.CreateDirectory(AppDataPath);
                }

                // 确保日志目录存在（在应用程序目录下）
                var logPath = LogPath;
                if (!Directory.Exists(logPath))
                {
                    Directory.CreateDirectory(logPath);
                }

                // 在调试模式下输出路径信息
                if (IsDebugMode)
                {
                    System.Diagnostics.Debug.WriteLine($"应用数据目录: {AppDataPath}");
                    System.Diagnostics.Debug.WriteLine($"日志目录: {logPath}");
                    System.Diagnostics.Debug.WriteLine($"应用程序目录: {AppDomain.CurrentDomain.BaseDirectory}");
                }
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"创建目录失败: {ex.Message}");
            }
        }

        /// <summary>
        /// 转换配置值类型
        /// </summary>
        /// <typeparam name="T">目标类型</typeparam>
        /// <param name="value">原始值</param>
        /// <param name="defaultValue">默认值</param>
        /// <returns>转换后的值</returns>
        private T ConvertValue<T>(object value, T defaultValue)
        {
            try
            {
                if (value == null)
                    return defaultValue;

                var targetType = typeof(T);

                // 处理可空类型
                if (targetType.IsGenericType && targetType.GetGenericTypeDefinition() == typeof(Nullable<>))
                {
                    targetType = Nullable.GetUnderlyingType(targetType);
                }

                // 特殊处理复杂类型
                if (targetType == typeof(WindowSettings))
                {
                    if (value is string jsonString)
                    {
                        return JsonConvert.DeserializeObject<T>(jsonString);
                    }
                    else if (value is WindowSettings)
                    {
                        return (T)value;
                    }
                    return defaultValue;
                }

                // 处理基本类型转换
                if (targetType.IsEnum)
                {
                    return (T)Enum.Parse(targetType, value.ToString(), true);
                }

                return (T)Convert.ChangeType(value, targetType);
            }
            catch
            {
                return defaultValue;
            }
        }

        /// <summary>
        /// 获取默认日志路径
        /// </summary>
        /// <returns>日志路径</returns>
        private string GetDefaultLogPath()
        {
            try
            {
                // 获取应用程序执行目录
                var appDirectory = AppDomain.CurrentDomain.BaseDirectory;
                return Path.Combine(appDirectory, "Logs");
            }
            catch
            {
                // 如果获取失败，使用应用数据目录作为备选
                return Path.Combine(AppDataPath, "Logs");
            }
        }

        /// <summary>
        /// 获取应用程序版本
        /// </summary>
        /// <returns>版本号</returns>
        private string GetApplicationVersion()
        {
            try
            {
                var version = System.Reflection.Assembly.GetExecutingAssembly().GetName().Version;
                return $"{version.Major}.{version.Minor}.{version.Build}";
            }
            catch
            {
                return "1.0.0";
            }
        }

        /// <summary>
        /// 触发配置变更事件
        /// </summary>
        /// <param name="key">配置键</param>
        /// <param name="oldValue">旧值</param>
        /// <param name="newValue">新值</param>
        private void OnConfigurationChanged(string key, object oldValue, object newValue)
        {
            try
            {
                ConfigurationChanged?.Invoke(this, new ConfigurationChangedEventArgs
                {
                    Key = key,
                    OldValue = oldValue,
                    NewValue = newValue
                });
            }
            catch
            {
                // 忽略事件处理错误
            }
        }

        #endregion
    }
}