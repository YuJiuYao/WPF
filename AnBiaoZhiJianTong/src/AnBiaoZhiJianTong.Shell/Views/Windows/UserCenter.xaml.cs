using System;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Input;
using AnBiaoZhiJianTong.Shell.ViewModels;
using Prism.Events;
using Prism.Ioc;

namespace AnBiaoZhiJianTong.Shell.Views.Windows
{
    /// <summary>
    /// UserCenter.xaml 的交互逻辑
    /// </summary>
    public partial class UserCenter : Window
    {
        public UserCenter()
        {
            InitializeComponent();
            CenterWindow();
        }
        private void CenterWindow()
        {
            // 获取屏幕的工作区域
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
