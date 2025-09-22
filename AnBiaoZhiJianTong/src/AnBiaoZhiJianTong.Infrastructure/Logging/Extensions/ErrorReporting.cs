using System;
using System.Threading.Tasks;
using System.Windows;
using AnBiaoZhiJianTong.Common.Utilities;

namespace AnBiaoZhiJianTong.Infrastructure.Logging.Extensions
{
    internal class ErrorReporting
    {
        /*public static void DispatcherUnhandledException(object sender, System.Windows.Threading.DispatcherUnhandledExceptionEventArgs e)
        {
            e.Handled = true;
            LogHelper.Log.Error("Current_DispatcherUnhandledException", e.Exception);
            MessageBox.Show($"程序出现异常{Environment.NewLine}{e.Exception.Message}");
        }

        public static void UnhandledException(object sender, UnhandledExceptionEventArgs e)
        {
            LogHelper.Log.Error("CurrentDomain_UnhandledException", e.ExceptionObject as Exception);
            MessageBox.Show($"程序出现异常{Environment.NewLine}{(e.ExceptionObject as Exception).Message}");
        }

        public static void UnobservedTaskException(object sender, UnobservedTaskExceptionEventArgs e)
        {
            LogHelper.Log.Error("TaskScheduler_UnobservedTaskException", e.Exception);
            MessageBox.Show($"程序出现异常{Environment.NewLine}{e.Exception.Message}");
        }
        */


    }
}
