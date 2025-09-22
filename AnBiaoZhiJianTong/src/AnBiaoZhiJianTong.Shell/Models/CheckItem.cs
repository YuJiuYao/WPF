using Prism.Mvvm;

namespace AnBiaoZhiJianTong.Shell.Models
{
    public class CheckItem : BindableBase
    {
        private string _itemName;
        public string ItemName
        {
            get => _itemName;
            set => SetProperty(ref _itemName, value);
        }

        private string _checkContent; // 第二列要动态刷新
        public string CheckContent
        {
            get => _checkContent;
            set => SetProperty(ref _checkContent, value);
        }

        private bool _isHeader;
        public bool IsHeader
        {
            get => _isHeader;
            set => SetProperty(ref _isHeader, value);
        }

        private string _backgroundColor = "Transparent";
        public string BackgroundColor
        {
            get => _backgroundColor;
            set => SetProperty(ref _backgroundColor, value);
        }
    }
}
