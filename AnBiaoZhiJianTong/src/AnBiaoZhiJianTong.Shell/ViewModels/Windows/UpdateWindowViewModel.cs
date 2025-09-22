using System;
using System.ComponentModel;
using System.Diagnostics;
using System.Globalization;
using System.IO;
using System.Net;
using System.Net.Http;
using System.Security.Cryptography;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Input;
using AnBiaoZhiJianTong.Core.Contracts.Platform;
using AnBiaoZhiJianTong.Models.UpdateDTO;
using Prism.Commands;

namespace AnBiaoZhiJianTong.Shell.ViewModels.Windows
{
    internal class UpdateWindowViewModel : INotifyPropertyChanged
    {
        /// <summary>当前已安装的版本号（供 UI 显示）</summary>
        private string _currentVersion;
        public string CurrentVersion
        {
            get => _currentVersion;
            set { _currentVersion = value; OnPropertyChanged(nameof(CurrentVersion)); }
        }


        /// <summary>服务器返回的新版本号（供 UI 显示）</summary>
        private string _latestVersion;
        public string LatestVersion
        {
            get => _latestVersion;
            set { _latestVersion = value; OnPropertyChanged(nameof(LatestVersion)); }
        }

        private string _upgradeDescription = "正在获取更新说明…";
        public string UpgradeDescription
        {
            get => _upgradeDescription;
            set { _upgradeDescription = value; OnPropertyChanged(nameof(UpgradeDescription)); }
        }

        /// <summary>下载状态提示文案</summary>
        private string _statusText = "准备下载...";
        public string StatusText
        {
            get => _statusText;
            set { _statusText = value; OnPropertyChanged(nameof(StatusText)); }
        }


        /// <summary>下载进度 0~100（当未知总大小时由 IsIndeterminate 控制为“循环模式”）</summary>
        private double _progressValue;
        public double ProgressValue
        {
            get => _progressValue;
            set { _progressValue = value; OnPropertyChanged(nameof(ProgressValue)); }
        }


        /// <summary>是否启用“循环进度条”（当无法获取 Content-Length 或后端未提供 fileSize 时）</summary>
        private bool _isIndeterminate = true;
        public bool IsIndeterminate
        {
            get => _isIndeterminate;
            set { _isIndeterminate = value; OnPropertyChanged(nameof(IsIndeterminate)); }
        }


        /// <summary>下载速度与已下载/总大小显示文本</summary>
        private string _speedText = "";
        public string SpeedText
        {
            get => _speedText;
            set { _speedText = value; OnPropertyChanged(nameof(SpeedText)); }
        }


        /// <summary>“开始安装”按钮是否可用（下载完成后置为 true）</summary>
        private bool _isInstallEnabled;
        public bool IsInstallEnabled
        {
            get => _isInstallEnabled;
            set { _isInstallEnabled = value; OnPropertyChanged(nameof(IsInstallEnabled)); }
        }


        /// <summary>下载取消令牌源（点击取消时触发）</summary>
        private CancellationTokenSource _cts;

        /// <summary>最终下载到的本地文件完整路径</summary>
        private string _downloadPath;

        /// <summary>更新信息模型（由调用方传入）</summary>
        private LatestVersionInfo LatestVersionInfo { get; }

        // ===== 新增：请求关窗事件 =====
        public event EventHandler RequestClose;
        // 防重入标记：确保取消逻辑只执行一次
        private int _cancelHandled = 0;

        /// <summary>内部状态：是否正在下载</summary>
        private volatile bool _isDownloading;
        private readonly INotifyStateService _notifyStateService; // 新增

        public event PropertyChangedEventHandler PropertyChanged;
        /// <summary>
        /// 属性变更通知。
        /// </summary>
        /// <param name="name"></param>
        private void OnPropertyChanged(string name) => PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(name));


        #region 派生属性（方便选择包/校验值）
        private string PackageUrl =>
            !string.IsNullOrWhiteSpace(LatestVersionInfo?.FullPackageUrl)
                ? LatestVersionInfo.FullPackageUrl
                : null;

        private string PackageMd5 =>
            !string.IsNullOrWhiteSpace(LatestVersionInfo?.FullPackageMd5)
                ? LatestVersionInfo.FullPackageMd5
                : LatestVersionInfo?.IncrementalPackageMd5;
        #endregion

        #region 命令
        public ICommand CancelCommand { get; }
        public ICommand InstallCommand { get; }
        #endregion

        
        public UpdateWindowViewModel(LatestVersionInfo info, string currentVersion, INotifyStateService notifyStateService)
        {
            LatestVersionInfo = info ?? throw new ArgumentNullException(nameof(info));
            _notifyStateService = notifyStateService ?? throw new ArgumentNullException(nameof(notifyStateService));
            CurrentVersion = currentVersion;
            LatestVersion = info.Version ?? "";
            UpgradeDescription = info.Description;

            CancelCommand = new DelegateCommand(OnCancel);
            InstallCommand = new DelegateCommand(OnInstall, () => IsInstallEnabled)
                .ObservesProperty(() => IsInstallEnabled);
        }

        public async Task StartDownloadAsync()
        {
            if (string.IsNullOrWhiteSpace(PackageUrl))
            {
                StatusText = "下载地址为空，请联系工作人员";
                return;
            }

            var folder = Path.Combine(Path.GetTempPath(), "AnBiaoZhiJianTong", "updates");
            Directory.CreateDirectory(folder);

            string ext;
            try
            {
                var u = new Uri(PackageUrl);
                ext = Path.GetExtension(u.AbsolutePath);
                if (string.IsNullOrWhiteSpace(ext)) ext = ".exe";
            }
            catch
            {
                ext = ".exe";
            }

            _downloadPath = Path.Combine(folder, $"AnBiaoZhiJianTong_{LatestVersion}{ext}");
            _cts = new CancellationTokenSource();
            _isDownloading = true;

            // —— 开始下载时显示遮罩（交接自 App）——
            _notifyStateService.Show("正在下载更新...");
            try
            {
                await DownloadAsync(PackageUrl, _downloadPath, _cts.Token);

                // 可选：MD5 校验（若服务器提供）
                if (!string.IsNullOrWhiteSpace(PackageMd5))
                {
                    StatusText = "正在校验...";
                    var ok = await VerifyMd5Async(_downloadPath, PackageMd5);
                    if (!ok)
                    {
                        StatusText = "MD5 校验失败";
                        return;
                    }
                }

                StatusText = "下载完成，可开始安装";
                _notifyStateService.Update("下载完成，可开始安装");
                IsInstallEnabled = true;
            }
            catch (OperationCanceledException)
            {
                StatusText = "已取消下载";
                _notifyStateService.Update("已取消下载");
                ShowCancelDownloadWarningAndExit();
            }
            catch (Exception ex)
            {
                StatusText = $"下载失败: {ex.Message}";
                _notifyStateService.Update("下载失败");
            }
            finally
            {
                _isDownloading = false;
            }
        }


        private async Task DownloadAsync(string url, string filePath, CancellationToken ct)
        {
            StatusText = "正在连接...";
            IsIndeterminate = true;
            _notifyStateService.Update("正在连接更新服务器...");

            // HttpClientHandler (支持自动解压缩)
            var handler = new HttpClientHandler
            {
                AutomaticDecompression = DecompressionMethods.GZip | DecompressionMethods.Deflate
            };

            using (var http = new HttpClient(handler))
            {
                http.Timeout = TimeSpan.FromMinutes(30);
                using (var resp = await http.GetAsync(url, HttpCompletionOption.ResponseHeadersRead, ct))
                {
                    resp.EnsureSuccessStatusCode();

                    var total = resp.Content.Headers.ContentLength ?? LatestVersionInfo.FileSize ?? 0;
                    IsIndeterminate = total <= 0;

                    var tmp = filePath + ".downloading";
                    if (File.Exists(tmp)) File.Delete(tmp);

                    const int bufferSize = 81920;
                    var buffer = new byte[bufferSize];
                    long received = 0;
                    var sw = Stopwatch.StartNew();
                    long lastReport = 0;

                    using (var input = await resp.Content.ReadAsStreamAsync())
                    using (var output = new FileStream(tmp, FileMode.Create, FileAccess.Write, FileShare.None, bufferSize, true))
                    {
                        StatusText = "下载中...";
                        while (true)
                        {
                            var read = await input.ReadAsync(buffer, 0, buffer.Length, ct);
                            if (read == 0) break;

                            await output.WriteAsync(buffer, 0, read, ct);
                            received += read;

                            if (total > 0)
                                ProgressValue = Math.Round(received * 100.0 / total, 2);

                            var now = sw.ElapsedMilliseconds;
                            if (now - lastReport > 200)
                            {
                                var totalTxt = total > 0 ? FormatSize(total) : "未知";
                                var speed = FormatSpeed(received, sw.Elapsed);
                                SpeedText = $"{FormatSize(received)} / {totalTxt}  ·  {speed}";
                                
                                _notifyStateService.Update($"正在下载更新… {FormatSize(received)} / {totalTxt} · {speed}");

                                lastReport = now;
                            }
                        }
                    }

                    if (File.Exists(filePath)) File.Delete(filePath);
                    File.Move(tmp, filePath);
                }
            }
        }

        private static async Task<bool> VerifyMd5Async(string file, string expectMd5)
        {
            try
            {
                using (var md5 = MD5.Create())
                using (var stream = File.OpenRead(file))
                {
                    var hash = await Task.Run(() => md5.ComputeHash(stream));
                    var actual = BitConverter.ToString(hash).Replace("-", "").ToLowerInvariant();
                    return string.Equals(actual, expectMd5.Trim().ToLowerInvariant(), StringComparison.OrdinalIgnoreCase);
                }
            }
            catch
            {
                return false;
            }
        }

        private static string FormatSize(long bytes)
        {
            string[] units = { "B", "KB", "MB", "GB", "TB" };
            double size = bytes;
            int unit = 0;
            while (size >= 1024 && unit < units.Length - 1) { size /= 1024; unit++; }
            return $"{size.ToString(size < 10 ? "0.##" : "0.#", CultureInfo.InvariantCulture)} {units[unit]}";
        }

        private static string FormatSpeed(long bytes, TimeSpan elapsed)
        {
            if (elapsed.TotalSeconds < 0.2) return "-- MB/s";
            var bps = bytes / elapsed.TotalSeconds;
            return $"{FormatSize((long)bps)}/s";
        }

        private void OnCancel()
        {
            // 确保只处理一次
            if (Interlocked.Exchange(ref _cancelHandled, 1) != 0) return;

            if (_isDownloading)
            {
                // 正在下载：发出取消信号，抛 OperationCanceledException → 提示并退出
                RaiseRequestClose();
                _cts?.Cancel();
            }
            else
            {
                RaiseRequestClose();    // 直接关闭更新窗体
                // 不在下载（已失败/已完成/未开始）：直接走提示并退出
                ShowCancelDownloadWarningAndExit();
            }
        }

        private void OnInstall()
        {
            if (string.IsNullOrEmpty(_downloadPath) || !File.Exists(_downloadPath))
            {
                StatusText = "安装包不存在";
                return;
            }

            try
            {
                _notifyStateService.Update($"正在启动安装程序...");

                var psi = new ProcessStartInfo
                {
                    FileName = _downloadPath,
                    UseShellExecute = true,
                    Verb = "runas" // 尝试提权
                };
                Process.Start(psi);
                Environment.Exit(0);
            }
            catch (Exception ex)
            {
                StatusText = $"启动安装失败：{ex.Message}";
                _notifyStateService.Update("启动安装失败");
            }
        }

        public void Dispose()
        {
            _cts?.Cancel();
            _cts?.Dispose();
        }


        private void ShowCancelDownloadWarningAndExit()
        {
            // 1) 清理半成品
            try
            {
                if (!string.IsNullOrEmpty(_downloadPath))
                {
                    var tmp = _downloadPath + ".downloading";
                    if (File.Exists(tmp)) File.Delete(tmp);
                }
            }
            catch { /* 忽略清理异常 */ }

            var app = Application.Current;
            if (app == null)
            {
                // 极端场景：没有 WPF Application，直接退出
                Environment.Exit(1);
                return;
            }

            // 2) 在 UI 线程异步显示一个 3 秒自动消失的小提示窗
            app.Dispatcher.BeginInvoke(new Action(delegate
            {
                // 小提示内容
                var border = new System.Windows.Controls.Border
                {
                    CornerRadius = new CornerRadius(8),
                    Background = new System.Windows.Media.SolidColorBrush(
                        System.Windows.Media.Color.FromArgb(230, 0, 0, 0)), // 半透明黑
                    Padding = new Thickness(18)
                };

                var text = new System.Windows.Controls.TextBlock
                {
                    Text = "您已取消下载安装最新版本。\n程序即将退出...",
                    Foreground = System.Windows.Media.Brushes.White,
                    FontSize = 14,
                    TextWrapping = TextWrapping.Wrap
                };
                border.Child = text;

                var tip = new Window
                {
                    WindowStyle = WindowStyle.None,
                    AllowsTransparency = true,
                    Background = System.Windows.Media.Brushes.Transparent,
                    ShowInTaskbar = false,
                    Topmost = true,
                    SizeToContent = SizeToContent.WidthAndHeight,
                    WindowStartupLocation = WindowStartupLocation.CenterScreen, // 避免 Owner 为空导致不显示
                    Content = border
                };

                // 显示提示
                tip.Show();

                // 3) 3 秒后关闭提示并退出（用 DispatcherTimer 确保在 UI 线程执行）
                var timer = new System.Windows.Threading.DispatcherTimer
                {
                    Interval = TimeSpan.FromSeconds(3)
                };
                timer.Tick += delegate
                {
                    try { timer.Stop(); } catch { }
                    try { tip.Close(); } catch { }
                    try { app.Shutdown(); } catch { }
                    Environment.Exit(1);
                };
                timer.Start();
            }));
        }

        private void RaiseRequestClose()
        {
            Application.Current?.Dispatcher?.BeginInvoke(new Action(() =>
            {
                RequestClose?.Invoke(this, EventArgs.Empty);
            }));
        }

    }
}
