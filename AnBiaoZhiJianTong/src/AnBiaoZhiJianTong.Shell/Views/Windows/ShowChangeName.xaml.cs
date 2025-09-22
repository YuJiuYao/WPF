using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;

namespace AnBiaoZhiJianTong.Shell.Views.Windows
{
    /// <summary>
    /// ShowChangeName.xaml 的交互逻辑
    /// </summary>
    public partial class ShowChangeName : Window
    {
        public string EnteredName { get; private set; } = string.Empty;
        public ShowChangeName()
        {
            InitializeComponent();
        }
        private void Save_Click(object sender, RoutedEventArgs e)
        {
            EnteredName = NameBox.Text?.Trim();
            DialogResult = true;   // 关闭对话框并返回 true
        }

        private void Cancel_Click(object sender, RoutedEventArgs e)
        {
            DialogResult = false;  // 关闭对话框并返回 false
        }
    }
}
