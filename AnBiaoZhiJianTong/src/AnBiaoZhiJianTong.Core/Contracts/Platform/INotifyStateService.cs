using System;
using System.ComponentModel;

namespace AnBiaoZhiJianTong.Core.Contracts.Platform
{
    /// <summary>
    /// 遮罩状态服务接口
    /// 用于全局控制主窗体上的等待遮罩（是否显示、提示文本）
    /// </summary>
    public interface INotifyStateService : INotifyPropertyChanged
    {
        /// <summary>
        /// 遮罩是否可见（true = 显示全局遮罩，false = 隐藏）
        /// </summary>
        bool IsGlobalMaskVisible { get; }
        /// <summary>
        /// 遮罩显示的提示文本（如“正在加载…”、“正在检查更新…”）
        /// </summary>
        string NotifyTextStatus { get; }

        /// <summary>
        /// 显示遮罩（可选：设置提示文案）
        /// </summary>
        void Show(string text = null);

        /// <summary>
        /// 更新遮罩文案（不改变遮罩可见性）
        /// </summary>
        void Update(string text);

        /// <summary>
        /// 隐藏遮罩（保持最后的文案，或者你可以在实现里清空）
        /// </summary>
        void Hide();

        /// <summary>
        /// 创建一个作用域遮罩：
        /// - 构造时 Show()
        /// - Dispose() 时 Hide()
        /// 用于 using 块中，确保作用域结束后自动关闭遮罩
        /// </summary>
        IDisposable Scope(string text = null);
    }

    
}
