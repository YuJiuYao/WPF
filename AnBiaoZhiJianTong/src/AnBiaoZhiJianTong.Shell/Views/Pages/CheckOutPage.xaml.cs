using System.Windows;
using System.Windows.Controls;
using AnBiaoZhiJianTong.Shell.Models;
using AnBiaoZhiJianTong.Shell.ViewModels.Pages;
using Prism.Ioc;

namespace AnBiaoZhiJianTong.Shell.Views
{
    /// <summary>
    /// CheckOutPage.xaml 的交互逻辑
    /// </summary>
    public partial class CheckOutPage : Page
    {
        public CheckOutPage()
        {
            InitializeComponent();
            DataContext = ContainerLocator.Container.Resolve<CheckOutViewModel>();
        }
        private void TreeView_SelectedItemChanged(object sender, RoutedPropertyChangedEventArgs<object> e)
        {
            // 更新中间数据表格的内容
            var viewModel = DataContext as CheckOutViewModel;
            if (viewModel != null)
            {
                viewModel.SelectedItem = e.NewValue as MyTreeNode;
                if (viewModel.SelectedItem != null)
                {
                    ErrorCheckPage.Content = viewModel.SelectedItem.MiddleContent;
                }
            }
        }
        private void TreeView_Loaded(object sender, RoutedEventArgs e)
        {
            var viewModel = DataContext as CheckOutViewModel;

            if (viewModel != null && viewModel.RootItems.Count > 0)
            {
                var firstItem = viewModel.RootItems[0];

                // 1. 更新 SelectedItem
                viewModel.SelectedItem = firstItem;

                // 2. 手动触发选中变更事件（模拟真实的点击行为）
                TreeView_SelectedItemChanged(sender,
                    new RoutedPropertyChangedEventArgs<object>(
                        oldValue: null,
                        newValue: firstItem
                    ));
            }
        }
        public class CheckOutTemplateSelector : DataTemplateSelector
        {
            public DataTemplate ErrorDataTemplate { get; set; }

        }

    }
}
