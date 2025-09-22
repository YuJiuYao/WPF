using System;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using System.Windows;
using System.Windows.Threading;
using AnBiaoZhiJianTong.Core.Contracts.Platform;

namespace AnBiaoZhiJianTong.Infrastructure.Platform
{
    /// <summary>
    /// 遮罩状态服务的实现
    /// 通过 INotifyPropertyChanged 通知 UI（绑定到 MainWindow.xaml）
    /// </summary>
    public sealed class NotifyStateService : INotifyStateService
    {
        private bool _isGlobalMaskVisible;
        /// <inheritdoc/>
        public bool IsGlobalMaskVisible
        {
            get => _isGlobalMaskVisible;
            private set { _isGlobalMaskVisible = value; OnPropertyChanged(); }
        }


        private string _notifyTextStatus = string.Empty;
        /// <inheritdoc/>
        public string NotifyTextStatus
        {
            get => _notifyTextStatus;
            private set { _notifyTextStatus = value; OnPropertyChanged(); }
        }

        /// <summary>
        /// 属性变更通知事件（供 WPF 绑定刷新 UI）
        /// </summary>
        public event PropertyChangedEventHandler PropertyChanged;
        /// <summary>
        /// 触发属性变更通知
        /// </summary>
        private void OnPropertyChanged([CallerMemberName] string name = null)
            => PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(name));


        /// <summary>
        /// 在 UI 线程上执行指定操作
        /// 确保在后台线程调用时，能正确更新 WPF 绑定属性
        /// </summary>
        private static void OnUI(Action action)
        {
            var app = Application.Current;
            if (app?.Dispatcher?.CheckAccess() == true)
            {
                action();
            }
            else
            {
                app?.Dispatcher?.Invoke(action, DispatcherPriority.Normal);
            }
        }

        /// <inheritdoc/>
        public void Show(string text = null)
        {
            OnUI(() =>
            {
                if (!string.IsNullOrWhiteSpace(text))
                    NotifyTextStatus = text.Trim();
                IsGlobalMaskVisible = true;
            });
        }

        /// <inheritdoc/>
        public void Update(string text)
        {
            if (string.IsNullOrWhiteSpace(text)) return;
            OnUI(() => NotifyTextStatus = text.Trim());
        }

        /// <inheritdoc/>
        public void Hide()
        {
            OnUI(() =>
            {
                IsGlobalMaskVisible = false;
                // 文案是否清空看需求，这里保留最后一条
            });
        }

        /// <inheritdoc/>
        public IDisposable Scope(string text = null)
        {
            Show(text);
            return new ScopeDisposable(this);
        }


        /// <summary>
        /// 内部类：用于实现 Scope 的自动关闭遮罩
        /// </summary>
        private sealed class ScopeDisposable : IDisposable
        {
            private readonly NotifyStateService _svc;
            private bool _disposed;
            public ScopeDisposable(NotifyStateService svc) => _svc = svc;

            public void Dispose()
            {
                if (_disposed) return;
                _disposed = true;
                _svc.Hide();
            }
        }

    }
}
