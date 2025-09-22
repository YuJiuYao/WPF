using System;
using System.Text.RegularExpressions;
using System.Windows;
using System.Windows.Input;
using System.Windows.Navigation;
using AnBiaoZhiJianTong.Shell.ViewModels.Windows;

namespace AnBiaoZhiJianTong.Shell.Views.Windows
{
    public partial class LoginWindow : Window
    {
        public LoginWindow()
        {
            InitializeComponent();
            Loaded += LoginWindow_Loaded;
            DataContextChanged += LoginWindow_DataContextChanged;
        }

        private void LoginWindow_Loaded(object sender, RoutedEventArgs e)
        {
            PhoneTextBox.Focus();
        }

        private void LoginWindow_DataContextChanged(object sender, DependencyPropertyChangedEventArgs e)
        {
            if (e.OldValue is LoginWindowViewModel oldVm)
            {
                oldVm.RequestClose -= ViewModelOnRequestClose;
            }

            if (e.NewValue is LoginWindowViewModel newVm)
            {
                newVm.RequestClose += ViewModelOnRequestClose;
            }
        }

        private void ViewModelOnRequestClose(object sender, LoginWindowCloseEventArgs e)
        {
            DialogResult = e.IsSuccess;
        }

        private void TitleBar_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            if (e.ButtonState == MouseButtonState.Pressed)
            {
                DragMove();
            }
        }

        private void CloseButton_Click(object sender, RoutedEventArgs e)
        {
            Close();
        }

        private void PhoneTextBox_PreviewTextInput(object sender, TextCompositionEventArgs e)
        {
            e.Handled = !IsDigitsOnly(e.Text);
        }

        private void PhoneTextBox_OnPasting(object sender, DataObjectPastingEventArgs e)
        {
            if (e.DataObject.GetDataPresent(DataFormats.Text))
            {
                var text = e.DataObject.GetData(DataFormats.Text) as string;
                if (!IsDigitsOnly(text ?? string.Empty))
                {
                    e.CancelCommand();
                }
            }
            else
            {
                e.CancelCommand();
            }
        }

        private static bool IsDigitsOnly(string text) => Regex.IsMatch(text, "^\\d*$");

        protected override void OnClosed(EventArgs e)
        {
            if (DataContext is LoginWindowViewModel vm)
            {
                vm.RequestClose -= ViewModelOnRequestClose;
                vm.Dispose();
            }

            base.OnClosed(e);
        }

        private void Hyperlink_RequestNavigate(object sender, RequestNavigateEventArgs e)
        { 
            System.Diagnostics.Process.Start(new System.Diagnostics.ProcessStartInfo(e.Uri.AbsoluteUri) { UseShellExecute = true });
            e.Handled = true;
        }
    }
}
