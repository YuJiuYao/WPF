using System;
using System.Globalization;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Markup;
using AnBiaoZhiJianTong.Core.Contracts.Configuration;
using AnBiaoZhiJianTong.Core.Contracts.Features.Updates;
using AnBiaoZhiJianTong.Core.Contracts.Http;
using AnBiaoZhiJianTong.Core.Contracts.Http.Auth;
using AnBiaoZhiJianTong.Core.Contracts.Logging;
using AnBiaoZhiJianTong.Core.Contracts.Platform;
using AnBiaoZhiJianTong.Core.Contracts.Runtime;
using AnBiaoZhiJianTong.Core.Contracts.SQLite;
using AnBiaoZhiJianTong.Core.Contracts.SQLite.Mapper;
using AnBiaoZhiJianTong.Infrastructure.Configuration;
using AnBiaoZhiJianTong.Infrastructure.Features.Updates;
using AnBiaoZhiJianTong.Infrastructure.Http;
using AnBiaoZhiJianTong.Infrastructure.Http.Auth;
using AnBiaoZhiJianTong.Infrastructure.Logging;
using AnBiaoZhiJianTong.Infrastructure.Platform;
using AnBiaoZhiJianTong.Infrastructure.Runtime;
using AnBiaoZhiJianTong.Infrastructure.SQlLite;
using AnBiaoZhiJianTong.Infrastructure.SQlLite.Mapper;
using AnBiaoZhiJianTong.Shell.ShellUtilities;
using AnBiaoZhiJianTong.Shell.Views;
using AnBiaoZhiJianTong.Shell.Views.Pages;
using AnBiaoZhiJianTong.Shell.Views.Windows;
using Prism.DryIoc;
using Prism.Ioc;
using Prism.Modularity;
using Prism.Regions;

namespace AnBiaoZhiJianTong.Shell
{
    /// <summary>
    /// App.xaml 的交互逻辑 - 继承自 PrismApplication(只做壳)
    /// </summary>
    public partial class App : PrismApplication
    {
        protected override Window CreateShell() => Container.Resolve<MainWindow>();

        protected override void RegisterTypes(IContainerRegistry containerRegistry)
        {
            containerRegistry.RegisterSingleton<ILogger, SimpleLogger>();
            containerRegistry.RegisterSingleton<ISingleInstance, SingleInstance>();
            containerRegistry.RegisterSingleton<IAsposeLicenseService, AsposeLicenseService>();
            containerRegistry.RegisterSingleton<ICefBootstrapper, CefBootstrapper>();
            containerRegistry.RegisterSingleton<IAppDataBus, AppDataBus>();
            containerRegistry.RegisterSingleton<IDb3ContextProvider, Db3ContextProvider>();
            containerRegistry.RegisterSingleton<IDb3Initializer, Db3Initializer>();
            containerRegistry.Register<IDb3ContextFactory, Db3ContextFactory>();
            containerRegistry.Register<IAnBiaoDb3Repository, AnBiaoDb3Repository>();
            containerRegistry.RegisterSingleton<IUpdateService, UpdateService>();
            containerRegistry.RegisterSingleton<IAppConfiguration, AppConfiguration>();
            containerRegistry.RegisterSingleton<INotifyStateService, NotifyStateService>();
            // HTTP/Refit
            containerRegistry.RegisterSingleton<IApiClientFactory, RefitClientFactory>();
            // 通过工厂创建 IRefitZjtApi（唯一来源）
            containerRegistry.RegisterSingleton<IRefitZjtApi>(() =>
            {
                var cfg = Container.Resolve<IAppConfiguration>();
                var factory = Container.Resolve<IApiClientFactory>();
                return factory.CreateRefit<IRefitZjtApi>(cfg.ApiBaseUrl);
            });
            // 业务服务
            containerRegistry.RegisterSingleton<IAuthService, AuthService>();

            // 导航视图注册
            containerRegistry.RegisterForNavigation<ImportFilePage>();

            containerRegistry.Register<LoginWindow>();
        }


        protected override void ConfigureModuleCatalog(IModuleCatalog moduleCatalog)
        {
            // moduleCatalog.AddModule<Modules.Home.HomeModule>();
            // moduleCatalog.AddModule<Modules.Settings.SettingsModule>();
        }

        protected override void ConfigureRegionAdapterMappings(RegionAdapterMappings mappings)
        {
            base.ConfigureRegionAdapterMappings(mappings);

            // Prism.Wpf 已内置 FrameRegionAdapter，这里把 Frame 映射到它
            var factory = Container.Resolve<IRegionBehaviorFactory>();
            mappings.RegisterMapping(typeof(Frame), new FrameRegionAdapter(factory));
        }

        protected override void OnStartup(StartupEventArgs e)
        {
            // 强制 TLS1.2
            ServicePointManager.SecurityProtocol = SecurityProtocolType.Tls12;
            DispatcherUnhandledException += (s, args) =>
            {
                HandleGlobalException(args.Exception, "UI 线程未处理异常");
                args.Handled = true;
            };
            AppDomain.CurrentDomain.UnhandledException += (s, args2) =>
            {
                HandleGlobalException(args2.ExceptionObject as Exception, "AppDomain 未处理异常", args2.IsTerminating);
            };
            TaskScheduler.UnobservedTaskException += (s, args3) =>
            {
                HandleGlobalException(args3.Exception, "任务调度异常");
                args3.SetObserved();
            };

            base.OnStartup(e);

            var configuration = Container.Resolve<IAppConfiguration>();
            configuration.Initialize();

            var log = Container.Resolve<ILogger>();

            ApplyConfiguration(configuration, log);
            LogConfigurationSnapshot(configuration, log);

            // 单实例
            var single = Container.Resolve<ISingleInstance>();
            if (!single.AcquireMutex())
            {
                single.ShowAlreadyRunningNotice();
                single.BringExistingToFront();
                Current.Shutdown(0);
                return;
            }

            // 第三方运行时/许可
            Container.Resolve<IAsposeLicenseService>().EnsureLicensed();
            Container.Resolve<ICefBootstrapper>().Boot();
        }

        protected override void OnInitialized()
        {
            base.OnInitialized();

            var log = Container.Resolve<ILogger>();
            log.LogInfo("Shell initialized. 准备初始化数据库并执行首屏导航。");

            // DB 初始化
            Container.Resolve<IDb3Initializer>().Initialize();

            // 首屏导航
            var regionManager = Container.Resolve<IRegionManager>();
            if (regionManager == null)
            {
                log.LogError("无法解析 IRegionManager，导航 ImportFilePage 终止。");
                return;
            }

            var registeredRegions = string.Join(", ", regionManager.Regions.Select(r => r.Name));
            log.LogDebug($"当前已注册区域: {(string.IsNullOrWhiteSpace(registeredRegions) ? "<空>" : registeredRegions)}");

            if (!regionManager.Regions.ContainsRegionWithName(RegionNames.MainRegion))
            {
                log.LogWarning($"尚未发现名为 {RegionNames.MainRegion} 的区域，无法导航到 ImportFilePage。");
                return;
            }

            var mainRegion = regionManager.Regions[RegionNames.MainRegion];
            log.LogDebug($"导航前 {RegionNames.MainRegion} 区域中已有视图数量: {mainRegion.Views.Cast<object>().Count()}。");

            log.LogInfo("开始导航到 ImportFilePage。");
            regionManager.RequestNavigate(RegionNames.MainRegion, nameof(ImportFilePage), result =>
            {
                if (result.Result == true)
                {
                    log.LogInfo("导航到 ImportFilePage 成功。");
                }
                else if (result.Error != null)
                {
                    log.LogError(result.Error, "导航到 ImportFilePage 时出现异常。");
                }
                else
                {
                    log.LogWarning("导航到 ImportFilePage 返回失败但未提供异常信息。");
                }
            });

            regionManager.RequestNavigate(RegionNames.MainRegion, nameof(ImportFilePage));

            // 非阻塞的启动任务
            /*_ = Task.Run(async () =>
            {
                try
                {
                    // 例如：检查更新（UpdateService 内部决定是否弹窗：事件或 DialogService）
                    await Container.Resolve<IUpdateService>().CheckAndMaybePromptAsync();
                }
                catch (Exception ex) { log.Error(ex, "Startup background tasks failed."); }
            });*/
        }

        protected override void OnExit(ExitEventArgs e)
        {
            try
            {
                Container.Resolve<ICefBootstrapper>().Shutdown();
                Container.Resolve<ISingleInstance>().ReleaseMutex();
                Container.Resolve<ILogger>().LogInfo("Application exiting.");
            }
            catch { /* ignore */ }
            finally { base.OnExit(e); }
        }

        private void ApplyConfiguration(IAppConfiguration configuration, ILogger logger)
        {
            if (configuration == null)
            {
                return;
            }

            ApplyCulture(configuration, logger);
            ApplyTheme(configuration, logger);
        }

        private void ApplyCulture(IAppConfiguration configuration, ILogger logger)
        {
            if (string.IsNullOrWhiteSpace(configuration.Language))
            {
                return;
            }

            try
            {
                var culture = CultureInfo.GetCultureInfo(configuration.Language);
                Thread.CurrentThread.CurrentCulture = culture;
                Thread.CurrentThread.CurrentUICulture = culture;
                CultureInfo.DefaultThreadCurrentCulture = culture;
                CultureInfo.DefaultThreadCurrentUICulture = culture;
                FrameworkElement.LanguageProperty.OverrideMetadata(
                    typeof(FrameworkElement),
                    new FrameworkPropertyMetadata(XmlLanguage.GetLanguage(culture.IetfLanguageTag)));

                logger?.LogInfo($"UI 语言已应用：{culture.IetfLanguageTag}");
            }
            catch (CultureNotFoundException ex)
            {
                logger?.LogWarning($"无法应用语言设置 '{configuration.Language}'：{ex.Message}");
            }
        }

        private void ApplyTheme(IAppConfiguration configuration, ILogger logger)
        {
            if (string.IsNullOrWhiteSpace(configuration.Theme))
            {
                return;
            }

            try
            {
                /*if (Enum.TryParse<ApplicationTheme>(configuration.Theme, true, out var theme))
                {
                    ThemeManager.Current.ApplicationTheme = theme;
                    logger?.LogInfo($"主题已应用：{theme}");
                }
                else
                {
                    logger?.LogWarning($"未识别的主题配置：{configuration.Theme}");
                }*/
            }
            catch (Exception ex)
            {
                logger?.LogWarning($"应用主题时出现问题：{ex.Message}");
            }
        }

        private void LogConfigurationSnapshot(IAppConfiguration configuration, ILogger logger)
        {
            if (configuration == null || logger == null)
            {
                return;
            }

            var builder = new StringBuilder();
            builder.AppendLine("================ 应用配置快照 ================");
            builder.AppendLine("[基础]");
            builder.AppendLine($"API 基础地址: {configuration.ApiBaseUrl}");
            builder.AppendLine($"应用数据目录: {configuration.AppDataPath}");
            builder.AppendLine($"日志目录: {configuration.LogPath}");
            builder.AppendLine($"最大文件大小: {configuration.MaxFileSize} B");
            builder.AppendLine($"支持的文件类型: {string.Join(", ", configuration.SupportedFileExtensions ?? Array.Empty<string>())}");
            builder.AppendLine($"请求超时: {configuration.RequestTimeout.TotalSeconds} 秒");
            builder.AppendLine($"启用日志: {configuration.EnableLogging}");
            builder.AppendLine($"最小日志级别: {configuration.MinLogLevel}");
            builder.AppendLine($"调试模式: {configuration.IsDebugMode}");
            builder.AppendLine($"版本: {configuration.AppVersion}");
            builder.AppendLine($"软件名称: {configuration.AppName}");
            builder.AppendLine($"公司: {configuration.CompanyName}");
            builder.AppendLine();

            builder.AppendLine("[界面]");
            builder.AppendLine($"主题: {configuration.Theme}");
            builder.AppendLine($"语言: {configuration.Language}");
            builder.AppendLine($"显示搜索框: {configuration.ShowSearchBox}");
            builder.AppendLine($"最小化到托盘: {configuration.MinimizeToTray}");
            builder.AppendLine($"启用自动更新: {configuration.EnableAutoUpdate}");
            builder.AppendLine($"窗口设置: {FormatWindowSettings(configuration.WindowSettings)}");
            builder.AppendLine();

            builder.AppendLine("[业务]");
            builder.AppendLine($"默认检查规则: {string.Join(", ", configuration.DefaultCheckRuleIds ?? Array.Empty<int>())}");
            builder.AppendLine($"自动保存间隔: {configuration.AutoSaveInterval} 分钟");
            builder.AppendLine($"最大历史记录: {configuration.MaxHistoryCount}");
            builder.AppendLine($"启用文件缓存: {configuration.EnableFileCache}");
            builder.AppendLine($"缓存过期: {configuration.CacheExpirationHours} 小时");
            builder.AppendLine("=============================================");

            logger.LogInfo(builder.ToString());
        }

        private string FormatWindowSettings(WindowSettings settings)
        {
            if (settings == null)
            {
                return "未配置";
            }

            return $"尺寸={settings.Width}x{settings.Height}, 位置={settings.Left},{settings.Top}, 状态={settings.WindowState}, 记住位置={settings.RememberPosition}";
        }

        private void HandleGlobalException(Exception exception, string context, bool isTerminating = false)
        {
            if (exception == null)
            {
                exception = new Exception("未知异常");
            }

            try
            {
                Container.Resolve<ILogger>().LogError(exception, context);
            }
            catch
            {
                // ignored
            }

            var shouldContinue = ShowExceptionDialog(exception, context, isTerminating);

            if (!shouldContinue)
            {
                try
                {
                    Current?.Dispatcher?.Invoke(() => Current?.Shutdown(-1));
                }
                catch
                {
                    // ignored
                }
            }
        }

        private bool ShowExceptionDialog(Exception exception, string context, bool isTerminating)
        {
            bool continueRunning = true;

            void Show()
            {
                var messageBuilder = new StringBuilder();
                messageBuilder.AppendLine($"发生异常：{context}");
                messageBuilder.AppendLine(exception.Message);

                if (!string.IsNullOrWhiteSpace(exception.StackTrace))
                {
                    messageBuilder.AppendLine();
                    messageBuilder.AppendLine("详情：");
                    messageBuilder.AppendLine(exception.StackTrace);
                }

                messageBuilder.AppendLine();

                if (isTerminating)
                {
                    messageBuilder.AppendLine("程序无法继续运行，即将退出。");
                }
                else
                {
                    messageBuilder.AppendLine("是否继续运行?");
                }

                var buttons = isTerminating ? MessageBoxButton.OK : MessageBoxButton.YesNo;
                var defaultResult = isTerminating ? MessageBoxResult.OK : MessageBoxResult.Yes;
                var owner = Current?.MainWindow;
                MessageBoxResult result;

                if (owner != null)
                {
                    result = MessageBox.Show(
                        owner,
                        messageBuilder.ToString(),
                        "程序运行异常",
                        buttons,
                        MessageBoxImage.Error,
                        defaultResult);
                }
                else
                {
                    result = MessageBox.Show(
                        messageBuilder.ToString(),
                        "程序运行异常",
                        buttons,
                        MessageBoxImage.Error,
                        defaultResult);
                }

                continueRunning = !isTerminating && result == MessageBoxResult.Yes;
            }

            if (Current?.Dispatcher?.CheckAccess() == true)
            {
                Show();
            }
            else
            {
                Current?.Dispatcher?.Invoke(Show);
            }

            return continueRunning;
        }
    }
}
