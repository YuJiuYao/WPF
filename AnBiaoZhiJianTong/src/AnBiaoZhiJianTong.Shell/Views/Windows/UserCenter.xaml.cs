using System.Windows;
using System.Windows.Input;

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
        }

        /// <summary>
        /// 标题栏鼠标左键按下 - 用于拖拽窗口
        /// </summary>
        private void TitleBar_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            if (e.LeftButton == MouseButtonState.Pressed)
            {
                DragMove();
            }
        }

        /// <summary>
        /// 关闭按钮点击
        /// </summary>
        private void CloseButton_Click(object sender, RoutedEventArgs e)
        {
            Close();
        }
    }
}
