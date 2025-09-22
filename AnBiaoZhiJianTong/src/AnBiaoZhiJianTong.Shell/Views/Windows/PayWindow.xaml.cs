using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;
using AnBiaoZhiJianTong.Shell.Models;
using AnBiaoZhiJianTong.Shell.ViewModels.Pages;
using CefSharp;
using CefSharp.Wpf;
using Prism.Events;
using Prism.Ioc;

namespace AnBiaoZhiJianTong.Shell.Views.Windows
{
    /// <summary>
    /// PayWindow.xaml 的交互逻辑
    /// </summary>
    public partial class PayWindow : Window
    {
        public static PayUrlViewModel _payUrlViewModel;
        private CancellationTokenSource _cancellationTokenSource;
        private TaskCompletionSource<bool> _tcs;
        public event EventHandler Closed;
        public bool IsClosed { get; private set; }
        public PayWindow()
        {
            InitializeComponent();
            _tcs = new TaskCompletionSource<bool>();
            IEventAggregator eventAggregator = ContainerLocator.Container.Resolve<IEventAggregator>();
            var chromiumBrowser = this.chromiumBrowser;
            _payUrlViewModel = ContainerLocator.Container.Resolve<PayUrlViewModel>();
            DataContext = _payUrlViewModel;
            CenterWindow();
        }
        private void CenterWindow()
        {
            foreach (Window parentWindow in Application.Current.Windows)
            {
                if (parentWindow.Name == "TheMainWindow") // 根据窗体名称来判断
                {
                    double parentX = parentWindow.Left;
                    double parentY = parentWindow.Top;
                    double x = (parentWindow.Width - this.Width) / 2;
                    double y = (parentWindow.Height - this.Height) / 2;
                    this.Left = (int)(parentX + x);
                    this.Top = (int)(parentY + y);
                    break;
                }
            }
        }
        /// <summary>
        /// 标题栏鼠标左键按下 - 用于拖拽窗口
        /// </summary>
        private void TitleBar_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            if (e.LeftButton == MouseButtonState.Pressed)
            {
                this.DragMove();
            }
        }
        /// <summary>
        /// 关闭按钮点击
        /// </summary>
        private void CloseButton_Click(object sender, RoutedEventArgs e)
        {
            this.Close();
        }
    }
}
