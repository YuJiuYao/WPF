// IAppConfiguration.cs - 应用配置接口

using System;
using System.Collections.Generic;

namespace AnBiaoZhiJianTong.Core.Contracts.Configuration
{
    /// <summary>
    /// 应用配置服务接口
    /// </summary>
    public interface IAppConfiguration
    {
        #region 基础属性

        /// <summary>
        /// API基础地址
        /// </summary>
        string ApiBaseUrl { get; }

        /// <summary>
        /// 应用数据路径
        /// </summary>
        string AppDataPath { get; }

        /// <summary>
        /// 日志路径
        /// </summary>
        string LogPath { get; }

        /// <summary>
        /// 最大文件大小（字节）
        /// </summary>
        long MaxFileSize { get; }

        /// <summary>
        /// 支持的文件扩展名
        /// </summary>
        string[] SupportedFileExtensions { get; }

        /// <summary>
        /// 请求超时时间
        /// </summary>
        TimeSpan RequestTimeout { get; }

        /// <summary>
        /// 是否启用日志
        /// </summary>
        bool EnableLogging { get; }

        /// <summary>
        /// 最小日志级别
        /// </summary>
        string MinLogLevel { get; }

        /// <summary>
        /// 是否启用调试模式
        /// </summary>
        bool IsDebugMode { get; }

        /// <summary>
        /// 应用名称
        /// </summary>
        string AppName { get; }

        /// <summary>
        /// 应用程序版本
        /// </summary>
        string AppVersion { get; }

        /// <summary>
        /// 公司名称
        /// </summary>
        string CompanyName { get; }

        #endregion

        #region UI设置

        /// <summary>
        /// 主题名称
        /// </summary>
        string Theme { get; }

        /// <summary>
        /// 语言设置
        /// </summary>
        string Language { get; }

        /// <summary>
        /// 是否显示搜索框
        /// </summary>
        bool ShowSearchBox { get; }

        /// <summary>
        /// 是否最小化到系统托盘
        /// </summary>
        bool MinimizeToTray { get; }

        /// <summary>
        /// 是否启用自动更新
        /// </summary>
        bool EnableAutoUpdate { get; }

        /// <summary>
        /// 窗口状态设置
        /// </summary>
        WindowSettings WindowSettings { get; }

        #endregion

        #region 业务设置

        /// <summary>
        /// 默认检查规则ID列表
        /// </summary>
        int[] DefaultCheckRuleIds { get; }

        /// <summary>
        /// 自动保存间隔（分钟）
        /// </summary>
        int AutoSaveInterval { get; }

        /// <summary>
        /// 最大历史记录数量
        /// </summary>
        int MaxHistoryCount { get; }

        /// <summary>
        /// 是否启用文件缓存
        /// </summary>
        bool EnableFileCache { get; }

        /// <summary>
        /// 缓存过期时间（小时）
        /// </summary>
        int CacheExpirationHours { get; }

        #endregion

        #region 方法

        /// <summary>
        /// 初始化配置
        /// </summary>
        void Initialize();

        /// <summary>
        /// 获取配置值
        /// </summary>
        /// <typeparam name="T">值类型</typeparam>
        /// <param name="key">配置键</param>
        /// <param name="defaultValue">默认值</param>
        /// <returns>配置值</returns>
        T GetValue<T>(string key, T defaultValue = default);

        /// <summary>
        /// 设置配置值
        /// </summary>
        /// <typeparam name="T">值类型</typeparam>
        /// <param name="key">配置键</param>
        /// <param name="value">配置值</param>
        void SetValue<T>(string key, T value);

        /// <summary>
        /// 检查配置键是否存在
        /// </summary>
        /// <param name="key">配置键</param>
        /// <returns>是否存在</returns>
        bool ContainsKey(string key);

        /// <summary>
        /// 移除配置项
        /// </summary>
        /// <param name="key">配置键</param>
        /// <returns>是否成功移除</returns>
        bool RemoveKey(string key);

        /// <summary>
        /// 获取所有配置键
        /// </summary>
        /// <returns>配置键列表</returns>
        IEnumerable<string> GetAllKeys();

        /// <summary>
        /// 保存配置
        /// </summary>
        void Save();

        /// <summary>
        /// 重新加载配置
        /// </summary>
        void Reload();

        /// <summary>
        /// 重置为默认配置
        /// </summary>
        void ResetToDefaults();

        /// <summary>
        /// 导出配置到文件
        /// </summary>
        /// <param name="filePath">文件路径</param>
        /// <returns>是否成功</returns>
        bool ExportToFile(string filePath);

        /// <summary>
        /// 从文件导入配置
        /// </summary>
        /// <param name="filePath">文件路径</param>
        /// <returns>是否成功</returns>
        bool ImportFromFile(string filePath);

        #endregion

        #region 事件

        /// <summary>
        /// 配置变更事件
        /// </summary>
        event EventHandler<ConfigurationChangedEventArgs> ConfigurationChanged;

        #endregion
    }

    /// <summary>
    /// 窗口设置
    /// </summary>
    public class WindowSettings
    {
        /// <summary>
        /// 窗口宽度
        /// </summary>
        public double Width { get; set; } = 1400;

        /// <summary>
        /// 窗口高度
        /// </summary>
        public double Height { get; set; } = 800;

        /// <summary>
        /// 窗口左边距
        /// </summary>
        public double Left { get; set; } = -1;

        /// <summary>
        /// 窗口上边距
        /// </summary>
        public double Top { get; set; } = -1;

        /// <summary>
        /// 窗口状态
        /// </summary>
        public string WindowState { get; set; } = "Normal";

        /// <summary>
        /// 是否记住窗口位置
        /// </summary>
        public bool RememberPosition { get; set; } = true;
    }

    /// <summary>
    /// 配置变更事件参数
    /// </summary>
    public class ConfigurationChangedEventArgs : EventArgs
    {
        /// <summary>
        /// 配置键
        /// </summary>
        public string Key { get; set; }

        /// <summary>
        /// 旧值
        /// </summary>
        public object OldValue { get; set; }

        /// <summary>
        /// 新值
        /// </summary>
        public object NewValue { get; set; }

        /// <summary>
        /// 变更时间
        /// </summary>
        public DateTime ChangeTime { get; set; } = DateTime.Now;
    }
}
