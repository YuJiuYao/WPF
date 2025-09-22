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
using System.Windows.Navigation;
using System.Windows.Shapes;
using AnBiaoZhiJianTong.Shell.ViewModels.Pages;
using Prism.Ioc;

namespace AnBiaoZhiJianTong.Shell.Views
{
    /// <summary>
    /// ErrorShowPage.xaml 的交互逻辑
    /// </summary>
    public partial class ErrorShowPage : UserControl
    {
        public ErrorShowPage()
        {
            InitializeComponent();
            DataContext = ContainerLocator.Container.Resolve<ErrorShowViewModel>();
        }
    }
}
