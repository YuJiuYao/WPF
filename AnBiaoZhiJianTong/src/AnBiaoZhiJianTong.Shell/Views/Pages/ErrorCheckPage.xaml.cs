using System.Windows;
using System.Windows.Controls;
using AnBiaoZhiJianTong.Core.Contracts.Runtime;
using AnBiaoZhiJianTong.Shell.Models;
using AnBiaoZhiJianTong.Shell.ViewModels.Pages;
using Prism.Ioc;

namespace AnBiaoZhiJianTong.Shell.Views
{
    /// <summary>
    /// ErrorCheckPage.xaml 的交互逻辑
    /// </summary>
    public partial class ErrorCheckPage : UserControl
    {
        private readonly IAppDataBus _appDataBus;
        public ErrorCheckPage()
        {
            InitializeComponent();
            DataContext = ContainerLocator.Container.Resolve<ErrorCheckViewModel>();
            _appDataBus = ContainerLocator.Container.Resolve<IAppDataBus>();
        }
        private void TreeView_SelectedItemChanged(object sender, RoutedPropertyChangedEventArgs<object> e)
        {
            // 更新中间数据表格的内容
            var viewModel = DataContext as ErrorCheckViewModel;
            if (viewModel != null && e.NewValue is MyTreeNode selectedNode)
            {
                viewModel.SelectedItem = selectedNode;
                if (selectedNode != null)
                {
                    _appDataBus.Set("AssociatedDataItems", selectedNode.AssociatedDataItems);
                    ErrorShowPage.Content = viewModel.SelectedItem.RightContent;
                }
            }
        }
        private void TreeView_Loaded(object sender, RoutedEventArgs e)
        {
            var viewModel = DataContext as ErrorCheckViewModel;

            if (viewModel != null && viewModel.ErrorRootItems.Count > 0)
            {
                var firstItem = viewModel.ErrorRootItems[0];

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
    }
}
