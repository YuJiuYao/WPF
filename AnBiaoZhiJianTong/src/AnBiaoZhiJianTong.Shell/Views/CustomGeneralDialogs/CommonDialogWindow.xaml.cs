using System;
using System.Windows;
using System.Windows.Input;
using AnBiaoZhiJianTong.Shell.Models;
using AnBiaoZhiJianTong.Shell.ViewModels.CustomGeneralDialogs;

namespace AnBiaoZhiJianTong.Shell.Views.CustomGeneralDialogs
{
    /// <summary>
    /// CommonDialogWindow.xaml 的交互逻辑
    /// </summary>
    public partial class CommonDialogWindow : Window
    {
        private CommonDialogOptions Options { get; }
        private CommonDialogResponse Response { get; } = new CommonDialogResponse();

        public ICommand ButtonClickCommand { get; }

        public CommonDialogWindow(CommonDialogOptions options)
        {
            Options = options ?? throw new ArgumentNullException(nameof(options));
            ButtonClickCommand = new RelayCommand<CommonDialogResult>(OnButtonClicked);

            InitializeComponent();

            // 先设 DataContext
            DataContext = new CommonDialogWindowViewModel(options);

            // 再正确设置 Owner & 位置
            var owner = options.Owner ?? Application.Current?.MainWindow;

            if (owner != null && owner != this)
            {
                Owner = owner;                         // 绑定所有者
                WindowStartupLocation = WindowStartupLocation.CenterOwner;
                ShowInTaskbar = false;                 // 跟随 Owner，不上任务栏
            }
            else
            {
                WindowStartupLocation = WindowStartupLocation.CenterScreen;
                ShowInTaskbar = true;                  // 没有 Owner 时，上任务栏，避免“丢失”
                Topmost = true;                        // 可选：避免被其他窗口遮住
            }

            PreviewKeyDown += (s, e) =>
            {
                if (e.Key == Key.Escape)
                {
                    // 认为 Esc 等价于“暂时退出”
                    OnButtonClicked(CommonDialogResult.Secondary);
                    e.Handled = true;   // 防止默认 IsCancel 逻辑再触发一次
                }
            };
            Closing += (s, e) =>
            {
                if (Response.Result == CommonDialogResult.None)
                {
                    // 你也可以定义 Close 专用的枚举，如果想区分 X 和 暂时退出
                    Response.Result = CommonDialogResult.Secondary;
                }
            };
        }

        private void OnButtonClicked(CommonDialogResult result)
        {
            Response.Result = result;
            Response.CheckboxChecked = Options.CheckboxChecked;
            DialogResult = true;
            Close();
        }

        // 简单 RelayCommand
        private sealed class RelayCommand<T> : ICommand
        {
            private readonly Action<T> _action;
            public RelayCommand(Action<T> action) => _action = action;
            public bool CanExecute(object parameter) => true;
            public void Execute(object parameter) => _action((T)parameter);
            public event EventHandler CanExecuteChanged;
        }

        // 静态帮助方法，便于一行调用
        public static CommonDialogResponse Show(CommonDialogOptions options)
        {
            if (Application.Current?.Dispatcher?.CheckAccess() == true)
            {
                var dlg = new CommonDialogWindow(options);
                dlg.ShowDialog();
                return dlg.Response;
            }
            return Application.Current?.Dispatcher?.Invoke(() =>
            {
                var dlg = new CommonDialogWindow(options);
                dlg.ShowDialog();
                return dlg.Response;
            });
        }

        private void TitleBar_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            if (e.ButtonState == MouseButtonState.Pressed)
                DragMove();
        }

        private void CloseButton_Click(object sender, RoutedEventArgs e)
        {
            // 如果用户点右上角 X，按“取消/Secondary”处理
            if (Response.Result == Models.CommonDialogResult.None)
                Response.Result = Models.CommonDialogResult.Secondary;

            DialogResult = true;
            Close();
        }
    }
}
