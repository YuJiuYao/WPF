using System;
using System.Windows;
using AnBiaoZhiJianTong.Core.Contracts.Platform;
using CefSharp;
using CefSharp.Wpf;

namespace AnBiaoZhiJianTong.Infrastructure.Platform
{
    public sealed class CefBootstrapper : ICefBootstrapper
    {
        public void Boot()
        {
            var settings = new CefSettings
            {
                WindowlessRenderingEnabled = true,
                CommandLineArgsDisabled = true,
                LogSeverity = LogSeverity.Disable
            };
            CefSharpSettings.SubprocessExitIfParentProcessClosed = true;
            try
            {
                Cef.Initialize(settings, performDependencyCheck: true, browserProcessHandler: null);
            }
            catch (Exception ex)
            {
                MessageBox.Show($"CefSharp 启动失败: {ex.Message}");
                Shutdown();
            }
        }

        public void Shutdown()
        {
            Cef.Shutdown();
        }
    }
}
