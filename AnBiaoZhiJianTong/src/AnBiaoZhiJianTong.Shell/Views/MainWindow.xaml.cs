using System.Windows;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;
using System.Windows.Input;
using AnBiaoZhiJianTong.Core.Contracts.Http.Auth;
using AnBiaoZhiJianTong.Core.Events;
using Prism.Events;
using Prism.Ioc;

namespace AnBiaoZhiJianTong.Shell.Views
{
    /// <summary>
    /// MainWindow.xaml 的交互逻辑
    /// </summary>
    public partial class MainWindow : Window
    {
        private readonly IContainerProvider _container;
        private readonly IAuthService _authService;
        private readonly IEventAggregator _eventAggregator;

        public MainWindow()
        {
            InitializeComponent();

            _container = ContainerLocator.Container;
            if (_container != null)
            {
                _authService = _container.Resolve<IAuthService>();
                _eventAggregator = _container.Resolve<IEventAggregator>();
            }
        }

        private void AvatarButton_Click(object sender, RoutedEventArgs e)
        {
            if (sender is Button btn && btn.ContextMenu != null)
            {
                btn.ContextMenu.PlacementTarget = btn;
                btn.ContextMenu.IsOpen = true;
            }
        }

        private void MenuButton_Click(object sender, RoutedEventArgs e)
        {
            if (sender is Button button && button.ContextMenu != null)
            {
                button.ContextMenu.PlacementTarget = button;
                button.ContextMenu.Placement = PlacementMode.Bottom;
                button.ContextMenu.IsOpen = true;
                e.Handled = true;
            }
        }

        private void MinimizeButton_OnClick(object sender, RoutedEventArgs e)
        {
            WindowState = WindowState.Minimized;
        }

        private void CloseButton_OnClick(object sender, RoutedEventArgs e)
        {
            Close();
        }

        private void LoginMenuItem_Click(object sender, RoutedEventArgs e)
        {
            if (_authService != null && _authService.IsAuthenticated)
            {
                return;
            }

            ShowLoginDialog();
        }

        private void LogoutMenuItem_Click(object sender, RoutedEventArgs e)
        {
            if (_authService == null || _eventAggregator == null)
            {
                return;
            }

            if (!_authService.IsAuthenticated)
            {
                return;
            }

            _authService.Logout();
            _eventAggregator.GetEvent<LoginInfoEvent>().Publish(null);
        }

        private void UserCenterMenuItem_Click(object sender, RoutedEventArgs e)
        {
            if (_container == null)
            {
                return;
            }

            var userCenterWindow = _container.Resolve<Views.Windows.UserCenter>();
            userCenterWindow.Owner = this;
            userCenterWindow.WindowStartupLocation = WindowStartupLocation.CenterOwner;
            userCenterWindow.ShowDialog();
        }

        private void ShowLoginDialog()
        {
            if (_container == null)
            {
                return;
            }

            try
            {
                LoginMask.Visibility = Visibility.Visible;

                var loginWindow = _container.Resolve<Views.Windows.LoginWindow>();
                loginWindow.Owner = this;
                loginWindow.WindowStartupLocation = WindowStartupLocation.CenterOwner;
                loginWindow.ShowDialog();
            }
            finally
            {
                LoginMask.Visibility = Visibility.Collapsed;
            }
        }
    }
}
