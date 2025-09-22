using System;
using System.Diagnostics;
using System.Linq;
using System.Runtime.InteropServices;
using System.Threading;
using AnBiaoZhiJianTong.Core.Contracts.Runtime;

namespace AnBiaoZhiJianTong.Infrastructure.Runtime;

public sealed class SingleInstance : ISingleInstance
{
    // 固定 GUID，避免与别的程序冲突；Local\ 限制在同一用户会话（多用户登录时互不影响）
    private const string MutexName = @"Local\AnBiaoZhiJianTong_{A8C4A6C1-3A14-4E2B-9C0E-1B6E7E9F7A11}";
    private static Mutex _mutex;

    #region Win32
    private const int SwShow = 5;
    private const int SwRestore = 9;

    [DllImport("user32.dll")] private static extern bool SetForegroundWindow(IntPtr hWnd);
    [DllImport("user32.dll")] private static extern bool IsIconic(IntPtr hWnd);
    [DllImport("user32.dll")] private static extern bool ShowWindow(IntPtr hWnd, int nCmdShow);
    [DllImport("user32.dll")] private static extern bool IsWindow(IntPtr hWnd);
    #endregion

    public bool AcquireMutex()
    {
        _mutex = new Mutex(initiallyOwned: true, name: MutexName, createdNew: out var createdNew);
        return createdNew;
    }

    public void ReleaseMutex()
    {
        try { _mutex?.ReleaseMutex(); } catch { /*ignore*/ }
        try { _mutex?.Dispose(); } catch { /*ignore*/ }
        _mutex = null;
    }

    public void BringExistingToFront()
    {
        try
        {
            var current = Process.GetCurrentProcess();
            var process = Process.GetProcessesByName(current.ProcessName)
                .Where(p => p.Id != current.Id)
                .ToList();

            foreach (var p in process)
            {
                // 有些时候 MainWindowHandle 还没就绪，稍微等一下
                IntPtr h = IntPtr.Zero;
                for (int i = 0; i < 10 && (h == IntPtr.Zero || !IsWindow(h)); i++)
                {
                    p.Refresh();
                    h = p.MainWindowHandle;
                    if (h != IntPtr.Zero && IsWindow(h)) break;
                    Thread.Sleep(100);
                }
                if (h == IntPtr.Zero || !IsWindow(h)) continue;

                // 还原 + 置前
                if (IsIconic(h)) ShowWindow(h, SwRestore);
                ShowWindow(h, SwShow);
                SetForegroundWindow(h);
                // 找到一个就够了
                break;
            }
        }
        catch { /* 忽略激活失败 */ }
    }

    public void ShowAlreadyRunningNotice()
    {
        

        // 复用你原 CommonDialogWindow 或简单 MessageBox
        System.Windows.MessageBox.Show("程序已在运行，无需再次打开。", "提示");
    }
}