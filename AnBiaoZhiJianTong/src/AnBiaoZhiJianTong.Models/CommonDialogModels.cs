using System.Collections.Generic;
using System.Windows;

namespace AnBiaoZhiJianTong.Shell.Models
{
    public enum CommonDialogIcon
    {
        None,
        Info,
        Success,
        Warning,
        Error,
        Question
    }

    public enum CommonDialogResult
    {
        None,
        Primary,   // 一般代表“确定/更新/执行”
        Secondary, // 一般代表“取消/稍后/否”
        Tertiary   // 第三个按钮（可选）
    }

    public sealed class CommonDialogButton
    {
        public string Text { get; set; } = "确定";
        public CommonDialogResult Result { get; set; } = CommonDialogResult.Primary;
        public bool IsDefault { get; set; } = false; // Enter
        public bool IsCancel { get; set; } = false;  // Esc
        public double MinWidth { get; set; } = 88;
    }

    public sealed class CommonDialogOptions
    {
        public string Title { get; set; } = "提示";
        public string Header { get; set; } = "";      // 标题行主文案（可选，不填就只用窗口Title）
        public string Message { get; set; } = "";     // 主体内容
        public CommonDialogIcon Icon { get; set; } = CommonDialogIcon.None;

        // 勾选框
        public bool ShowCheckbox { get; set; } = false;
        public string CheckboxText { get; set; } = "不再提示";
        public bool CheckboxChecked { get; set; } = false;

        // 按钮（最多三枚）
        public IList<CommonDialogButton> Buttons { get; set; } = new List<CommonDialogButton>
        {
            new CommonDialogButton{ Text = "确定", Result = CommonDialogResult.Primary, IsDefault = true }
        };

        // 窗口拥有者（可选）
        public Window Owner { get; set; }
    }

    public sealed class CommonDialogResponse
    {
        public CommonDialogResult Result { get; set; } = CommonDialogResult.None;
        public bool CheckboxChecked { get; set; } = false;
    }
}
