using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO;
using AnBiaoZhiJianTong.Core.Contracts.Runtime;
using AnBiaoZhiJianTong.Shell.Models;
using AnBiaoZhiJianTong.Shell.ViewModels.Pages;
using AnBiaoZhiJianTong.Shell.Views;
using Newtonsoft.Json;
using Prism.Mvvm;

namespace AnBiaoZhiJianTong.Shell.ViewModels.Pages
{
    public class ErrorShowViewModel : BindableBase
    {
        public ObservableCollection<MyTreeNodeData> AssociatedDataItems { get; private set; }
        public int ErrorCount { get; private set; }
        public int MannalCont { get; private set; }
        public int FixCount { get; private set; }
        private readonly IAppDataBus _appDataBus;
        public ErrorShowViewModel(IAppDataBus appDataBus)
        {
            _appDataBus = appDataBus;
            if (appDataBus.TryGet("AssociatedDataItems", out ObservableCollection<MyTreeNodeData> data))
            {
                AssociatedDataItems = data;
                ShowError();
            }
            else
            {
                AssociatedDataItems = new ObservableCollection<MyTreeNodeData>();
                ShowError();
            }
        }
        public void ShowError()
        {
            ErrorCount = 0;
            MannalCont = 0;
            foreach (var item in AssociatedDataItems)
            {
                if (item.CheckResult=="错误")
                {
                    ErrorCount++;
                }
                if(item.CheckResult=="人工检查")
                {
                    MannalCont++;
                }
            }
            FixCount = AssociatedDataItems.Count - ErrorCount - MannalCont;
        }
    }
}

