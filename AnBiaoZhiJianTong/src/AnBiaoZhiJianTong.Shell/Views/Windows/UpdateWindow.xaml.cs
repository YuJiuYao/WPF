using System;
using System.ComponentModel;
using System.Windows;
using AnBiaoZhiJianTong.Shell.ViewModels.Windows;
using Prism.Commands;

namespace AnBiaoZhiJianTong.Shell.Views.Windows
{
    /// <summary>
    /// UpdateWindow.xaml 的交互逻辑
    /// </summary>
    public partial class UpdateWindow : Window
    {
        private bool _allowClose = false;

        public UpdateWindow()
        {
            InitializeComponent();
            Loaded += UpdateWindow_Loaded;
            Closing += UpdateWindow_Closing;
            Closed += UpdateWindow_Closed;
        }

        private async void UpdateWindow_Loaded(object sender, RoutedEventArgs e)
        {
            if (DataContext is UpdateWindowViewModel vm)
            {
                // 订阅 VM 的关窗请求事件
                vm.RequestClose += Vm_RequestClose;

                await vm.StartDownloadAsync();
            }
        }

        private void UpdateWindow_Closing(object sender, CancelEventArgs e)
        {
            if (!(DataContext is UpdateWindowViewModel vm)) return;

            if (!_allowClose)
            {
                if (vm.CancelCommand is DelegateCommand cmd) cmd.Execute();
                e.Cancel = true;
                return;
            }

            // 允许关闭时不要再拦截
            e.Cancel = false;
        }

        private void Vm_RequestClose(object sender, EventArgs e)
        {
            _allowClose = true;
            Close(); // 关键：收到 VM 的请求，立刻关闭更新窗体
        }

        private void UpdateWindow_Closed(object sender, EventArgs e)
        {
            if (DataContext is UpdateWindowViewModel vm)
            {
                vm.RequestClose -= Vm_RequestClose; // 解绑，避免泄漏
            }
        }
    }
}
