using System.ComponentModel;
using System.Runtime.CompilerServices;
using AnBiaoZhiJianTong.Models.UpdateDTO;

namespace AnBiaoZhiJianTong.Shell.Models
{
    public sealed class UpdateDownloadContext : INotifyPropertyChanged
    {
        public LatestVersionInfo LatestVersionInfo { get; set; }
        public string CurrentVersion { get; set; }

        private double _progressValue;
        private string _statusText = "准备下载...";
        private string _speedText = "";
        private long _receivedBytes;
        private long _totalBytes;
        private bool _isIndeterminate;

        public double ProgressValue { get => _progressValue; set { _progressValue = value; OnPropertyChanged(); } }
        public string StatusText { get => _statusText; set { _statusText = value; OnPropertyChanged(); } }
        public string SpeedText { get => _speedText; set { _speedText = value; OnPropertyChanged(); } }
        public long ReceivedBytes { get => _receivedBytes; set { _receivedBytes = value; OnPropertyChanged(); } }
        public long TotalBytes { get => _totalBytes; set { _totalBytes = value; OnPropertyChanged(); } }
        public bool IsIndeterminate { get => _isIndeterminate; set { _isIndeterminate = value; OnPropertyChanged(); } }

        public event PropertyChangedEventHandler PropertyChanged;
        private void OnPropertyChanged([CallerMemberName] string name = null)
            => PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(name));
    }

}
