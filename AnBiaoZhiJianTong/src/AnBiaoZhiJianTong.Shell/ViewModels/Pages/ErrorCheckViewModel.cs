using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.IO;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Threading.Tasks;
using System.Windows.Markup;
using AnBiaoZhiJianTong.Core.Contracts.Runtime;
using AnBiaoZhiJianTong.Core.Events;
using AnBiaoZhiJianTong.Shell.Models;
using AnBiaoZhiJianTong.Shell.ViewModels.Pages;
using AnBiaoZhiJianTong.Shell.Views;
using Newtonsoft.Json;
using Prism.Mvvm;

namespace AnBiaoZhiJianTong.Shell.ViewModels.Pages
{
    public class ErrorCheckViewModel : BindableBase , INotifyPropertyChanged
    {
        //private ObservableCollection<MyTreeNode> _allRootItems;
        private ObservableCollection<MyTreeNode> _errorRootItems;
        public MyTreeNode SelectedItem { get; set; }
        //public ObservableCollection<MyTreeNode> AllRootItems
        //{
        //    get => _allRootItems;
        //    set => SetProperty(ref _allRootItems, value);
        //}
        public event PropertyChangedEventHandler PropertyChanged;
        protected virtual void OnPropertyChanged([CallerMemberName] string propertyName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
        public ObservableCollection<MyTreeNode> ErrorRootItems
        {
            get => _errorRootItems;
            private set
            {
                _errorRootItems = value;
                RefreshAllTotals();  // 集合被重置时更新统计
                OnPropertyChanged();
            }
        }
        private int _totalErrorCount;
        private int _totalManualCount;
        private int _totalFixCount;
        private int _totalCount;
        public int TotalErrorCount
        {
            get => _totalErrorCount;
            set
            {
                // 如果是直接修改总值（如通过UI编辑），可以在此分配差值到各子节点
                SetProperty(ref _totalErrorCount, value);
                UpdateTotalCount();  // 同步更新 TotalCount
            }
        }
        public int TotalManualCount
        {
            get => _totalManualCount;
            set
            {
                SetProperty(ref _totalManualCount, value);
                UpdateTotalCount();
            }
        }
        public int TotalCount
        {
            get => _totalCount;
            private set => SetProperty(ref _totalCount, value);  // 内部通过计算更新，禁止外部直接赋值
        }
        public int TotalFixCount
        {
            get => _totalFixCount;
            set
            {
                SetProperty(ref _totalFixCount, value);
            }
        }
        // 当 ErrorRootItems 变更时调用此方法（如集合增删/子节点数值修改）
        public void RefreshAllTotals()
        {
            TotalErrorCount = (ErrorRootItems?.Sum(node => node.ErrorCount) ?? 0)/2;
            TotalManualCount = (ErrorRootItems?.Sum(node => node.ManualCount) ?? 0)/2;
            // TotalCount 会自动更新，不需单独设置
        }
        private void UpdateTotalCount()
        {
            TotalCount = TotalErrorCount + TotalManualCount;
            _appDataBus.Set("TotalCount", TotalCount);
        }

        private readonly IAppDataBus _appDataBus;
        public ErrorCheckViewModel(IAppDataBus appDataBus)
        {
            _appDataBus = appDataBus;
            ErrorRootItems = new ObservableCollection<MyTreeNode>();
            //AllRootItems = new ObservableCollection<MyTreeNode>();
            _appDataBus.TryGet("TreeNode", out List<MyTreeNodeData> treeNode);
            BuildTreesAsync(treeNode).ConfigureAwait(false);
            _appDataBus.Set("ErrorRootItems", ErrorRootItems);
            _appDataBus.Set("TotalErrorCount", TotalErrorCount);
            _appDataBus.Set("TotalManualCount", TotalManualCount);
            TotalFixCount = _appDataBus.Get<int>("TotalCount") - TotalErrorCount - TotalManualCount;
        }

        private async Task BuildTreesAsync(List<MyTreeNodeData> treeNode)
        {
            //AllRootItems.Clear();
            ErrorRootItems.Clear();

            // 直接处理全部数据
            //AllRootItems = await MiddleAllBuildTree(treeNode);
            ErrorRootItems = await TreeBuilder.MiddleFaleBuildTree(treeNode);
        }
    }

}
