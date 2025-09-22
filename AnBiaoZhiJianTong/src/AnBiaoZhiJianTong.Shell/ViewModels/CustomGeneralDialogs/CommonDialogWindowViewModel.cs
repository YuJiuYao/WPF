using System.Linq;
using System.Windows;
using System.Windows.Media;
using AnBiaoZhiJianTong.Shell.Models;

namespace AnBiaoZhiJianTong.Shell.ViewModels.CustomGeneralDialogs
{
    internal class CommonDialogWindowViewModel
    {
        public string Title { get; }
        public string Header { get; }
        public string Message { get; }
        public string CheckboxText { get; }
        public bool CheckboxChecked { get; set; }
        // 修改：IList 便于索引
        public System.Collections.Generic.IList<CommonDialogButton> Buttons { get; }

        public CommonDialogButton FirstButton { get; }
        public CommonDialogButton SecondButton { get; }

        public Visibility IconVisibility { get; }
        public string IconGlyph { get; }
        public Brush IconBrush { get; }
        public Visibility CheckboxVisibility { get; }

        public CommonDialogWindowViewModel(CommonDialogOptions opt)
        {
            Title = string.IsNullOrWhiteSpace(opt.Title) ? "提示" : opt.Title;
            Header = string.IsNullOrWhiteSpace(opt.Header) ? Title : opt.Header;
            Message = opt.Message ?? "";
            CheckboxText = opt.CheckboxText ?? "不再提示";
            CheckboxChecked = opt.CheckboxChecked;
            // 复制到 List，保证 IList
            Buttons = opt.Buttons?.ToList() ?? new System.Collections.Generic.List<CommonDialogButton>();
            // 取前两个（没有就 null）
            FirstButton = Buttons.Count > 0 ? Buttons[0] : null;
            SecondButton = Buttons.Count > 1 ? Buttons[1] : null;

            CheckboxVisibility = opt.ShowCheckbox ? Visibility.Visible : Visibility.Collapsed;

            // 图标见下面第 2 点
            var tuple = IconHelper(opt.Icon);
            IconGlyph = tuple.glyph;
            IconVisibility = tuple.vis;
            IconBrush = tuple.brush;
        }
        private static (string glyph, Visibility vis, Brush brush) IconHelper(CommonDialogIcon icon)
        {
            string glyph;
            Visibility vis;
            Brush brush;

            switch (icon)
            {
                case CommonDialogIcon.Info:     // 蓝色
                    glyph = "\uE946";
                    vis = Visibility.Visible;
                    brush = Brushes.DodgerBlue;
                    break;
                case CommonDialogIcon.Success:  // 绿色
                    glyph = "\uE73E";
                    vis = Visibility.Visible;
                    brush = Brushes.ForestGreen;
                    break;
                case CommonDialogIcon.Warning:  // 琥珀色 (橙色/金色)
                    glyph = "\uE7BA";
                    vis = Visibility.Visible;
                    brush = Brushes.DarkOrange;
                    break;
                case CommonDialogIcon.Error:    // 红色
                    glyph = "\uEA39";
                    vis = Visibility.Visible;
                    brush = Brushes.IndianRed;
                    break;
                case CommonDialogIcon.Question: // 默认蓝色问号
                    glyph = "\uE9CE";
                    vis = Visibility.Visible;
                    brush = Brushes.SteelBlue;
                    break;
                default:
                    glyph = "";
                    vis = Visibility.Collapsed;
                    brush = Brushes.Transparent;
                    break;
            }

            return (glyph, vis, brush);
        }
    }
}
