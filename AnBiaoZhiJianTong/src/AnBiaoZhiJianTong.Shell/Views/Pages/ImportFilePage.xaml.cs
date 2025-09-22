using System;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using AnBiaoZhiJianTong.Core.Contracts.Logging;
using AnBiaoZhiJianTong.Shell.ViewModels.Pages;
using Prism.Ioc;

namespace AnBiaoZhiJianTong.Shell.Views.Pages
{
    /// <summary>
    /// ImportFilePage.xaml 的交互逻辑
    /// </summary>
    public partial class ImportFilePage : Page
    {
        public ImportFilePage()
        {
            InitializeComponent();

            var container = ContainerLocator.Container;
            if (container == null)
            {
                return;
            }

            var viewModel = container.Resolve<ImportFileViewModel>();
            DataContext = viewModel;

            ILogger logger = null;
            try
            {
                logger = container.Resolve<ILogger>();
            }
            catch (Exception)
            {
                // 忽略日志解析异常，避免影响视图构建。
            }

            logger?.LogInfo($"ImportFilePage 视图实例化完成，DataContext 类型: {viewModel?.GetType().FullName ?? "null"}。");

            DataContext = ContainerLocator.Container.Resolve<ImportFileViewModel>();
        }
        private void Border_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            if (sender is Border border && border.ContextMenu != null)
            {
                border.ContextMenu.PlacementTarget = border;  // 设置弹出位置
                border.ContextMenu.IsOpen = true;             // 强制打开
                e.Handled = true;                             
            }
        }

    }

    public class DisplayModeTemplateSelector : DataTemplateSelector
    {
        public DataTemplate  EmptyTemplate { get; set; }
        public DataTemplate  FileSelectedTemplate { get; set; }
        public override DataTemplate  SelectTemplate(object item, DependencyObject container)
        {
            if (item is ImportFileViewModel.DisplayMode mode)
            {
                switch (mode)
                {
                    case ImportFileViewModel.DisplayMode.Empty:
                        return EmptyTemplate;
                    case ImportFileViewModel.DisplayMode.FileSelected:
                        return FileSelectedTemplate;
                    default:
                        return null;
                }
            }
            return null;
        }
    }
}
