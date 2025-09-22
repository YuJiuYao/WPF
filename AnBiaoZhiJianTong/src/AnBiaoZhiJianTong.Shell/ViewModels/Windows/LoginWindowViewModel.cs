using System;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Media;
using System.Windows.Threading;
using AnBiaoZhiJianTong.Core.Contracts.Http.Auth;
using AnBiaoZhiJianTong.Core.Events;
using Prism.Commands;
using Prism.Events;
using Prism.Mvvm;

namespace AnBiaoZhiJianTong.Shell.ViewModels.Windows
{
    public class LoginWindowViewModel : BindableBase, IDisposable
    {
        private static readonly SolidColorBrush SuccessBrush;

        private readonly IAuthService _authService;
        private readonly IEventAggregator _eventAggregator;
        private readonly DispatcherTimer _countdownTimer;
        private int _secondsRemaining;


        private string _phoneNumber = string.Empty;
        public string PhoneNumber
        {
            get => _phoneNumber;
            set => SetProperty(ref _phoneNumber, value);
        }
        private string _verificationCode = string.Empty;
        public string VerificationCode
        {
            get => _verificationCode;
            set => SetProperty(ref _verificationCode, value);
        }
        private bool _isAutoLogin = true;
        public bool IsAutoLogin
        {
            get => _isAutoLogin;
            set => SetProperty(ref _isAutoLogin, value);
        }
        private bool _isRequestingCode;
        public bool IsRequestingCode
        {
            get => _isRequestingCode;
            private set => SetProperty(ref _isRequestingCode, value);
        }
        private bool _isCountingDown;
        public bool IsCountingDown
        {
            get => _isCountingDown;
            private set => SetProperty(ref _isCountingDown, value);
        }
        private bool _isLoggingIn;
        public bool IsLoggingIn
        {
            get => _isLoggingIn;
            private set => SetProperty(ref _isLoggingIn, value);
        }
        private string _statusMessage = string.Empty;
        public string StatusMessage
        {
            get => _statusMessage;
            private set => SetProperty(ref _statusMessage, value);
        }
        private Brush _statusBrush = Brushes.Transparent;
        public Brush StatusBrush
        {
            get => _statusBrush;
            private set => SetProperty(ref _statusBrush, value);
        }
        private string _requestCodeButtonText;
        public string RequestCodeButtonText
        {
            get => _requestCodeButtonText;
            private set => SetProperty(ref _requestCodeButtonText, value);
        }


        public DelegateCommand RequestCodeCommand { get; }
        public DelegateCommand LoginCommand { get; }

        public event EventHandler<LoginWindowCloseEventArgs> RequestClose;

        static LoginWindowViewModel()
        {
            SuccessBrush = new SolidColorBrush(Color.FromRgb(45, 107, 243));
            SuccessBrush.Freeze();
        }

        public LoginWindowViewModel(IAuthService authService, IEventAggregator eventAggregator)
        {
            _authService = authService ?? throw new ArgumentNullException(nameof(authService));
            _eventAggregator = eventAggregator ?? throw new ArgumentNullException(nameof(eventAggregator));

            _countdownTimer = new DispatcherTimer
            {
                Interval = TimeSpan.FromSeconds(1)
            };
            _countdownTimer.Tick += CountdownTimerOnTick;

            RequestCodeCommand = new DelegateCommand(
                    async () => await RequestCodeAsync(), CanRequestCode)
                .ObservesProperty(() => PhoneNumber)
                .ObservesProperty(() => IsRequestingCode)
                .ObservesProperty(() => IsCountingDown);

            LoginCommand = new DelegateCommand(
                    async () => await LoginAsync(), CanLogin)
                .ObservesProperty(() => PhoneNumber)
                .ObservesProperty(() => VerificationCode)
                .ObservesProperty(() => IsLoggingIn);

            RequestCodeButtonText = "获取验证码";
            StatusBrush = Brushes.Transparent;
        }


        private bool CanRequestCode()
        {
            return !IsRequestingCode && !IsCountingDown && !string.IsNullOrWhiteSpace(PhoneNumber);
        }

        private bool CanLogin()
        {
            return !IsLoggingIn
                   && !string.IsNullOrWhiteSpace(PhoneNumber)
                   && !string.IsNullOrWhiteSpace(VerificationCode);
        }

        private async Task RequestCodeAsync()
        {
            StatusMessage = string.Empty;
            StatusBrush = Brushes.Red;

            if (string.IsNullOrWhiteSpace(PhoneNumber))
            {
                StatusMessage = "请输入手机号";
                return;
            }

            IsRequestingCode = true;
            try
            {
                var response = await _authService.RequestSmsCodeAsync(PhoneNumber.Trim(), CancellationToken.None);
                if (response == null)
                {
                    StatusMessage = "验证码发送失败，请稍后重试";
                    return;
                }

                if (response.StatusCode == 200 || response.StatusCode == 0)
                {
                    StatusBrush = SuccessBrush;
                    StatusMessage = string.IsNullOrWhiteSpace(response.Message)
                        ? "验证码已发送，请注意查收"
                        : response.Message;
                    StartCountdown();
                }
                else
                {
                    StatusMessage = !string.IsNullOrWhiteSpace(response.Message)
                        ? response.Message
                        : "验证码发送失败，请稍后重试";
                }
            }
            catch (Exception ex)
            {
                StatusMessage = $"验证码发送失败：{ex.Message}";
            }
            finally
            {
                IsRequestingCode = false;
            }
        }

        private async Task LoginAsync()
        {
            StatusBrush = Brushes.Red;
            StatusMessage = string.Empty;

            if (string.IsNullOrWhiteSpace(PhoneNumber))
            {
                StatusMessage = "请输入手机号";
                return;
            }

            if (string.IsNullOrWhiteSpace(VerificationCode))
            {
                StatusMessage = "请输入验证码";
                return;
            }

            IsLoggingIn = true;
            try
            {
                var result = await _authService.LoginAsync(PhoneNumber.Trim(), VerificationCode.Trim(), CancellationToken.None);
                if (result == null)
                {
                    StatusMessage = "登录失败，请稍后重试";
                    return;
                }

                if (result.Data != null && (result.Code == 200 || result.Code == 0))
                {
                    _eventAggregator.GetEvent<LoginInfoEvent>().Publish(result.Data);
                    StatusMessage = string.Empty;
                    RequestClose?.Invoke(this, new LoginWindowCloseEventArgs(true));
                }
                else
                {
                    StatusMessage = !string.IsNullOrWhiteSpace(result.Message)
                        ? result.Message
                        : "登录失败，请检查验证码是否正确";
                }
            }
            catch (Exception ex)
            {
                StatusMessage = $"登录失败：{ex.Message}";
            }
            finally
            {
                IsLoggingIn = false;
            }
        }

        private void StartCountdown()
        {
            _secondsRemaining = 60;
            IsCountingDown = true;
            RequestCodeButtonText = $"{_secondsRemaining}s";
            _countdownTimer.Start();
        }

        private void CountdownTimerOnTick(object sender, EventArgs e)
        {
            _secondsRemaining--;
            if (_secondsRemaining > 0)
            {
                RequestCodeButtonText = $"{_secondsRemaining}s";
            }
            else
            {
                _countdownTimer.Stop();
                IsCountingDown = false;
                RequestCodeButtonText = "重新获取";
            }
        }

        public void Dispose()
        {
            _countdownTimer.Tick -= CountdownTimerOnTick;
            _countdownTimer.Stop();
        }
    }

    public class LoginWindowCloseEventArgs : EventArgs
    {
        public LoginWindowCloseEventArgs(bool isSuccess)
        {
            IsSuccess = isSuccess;
        }

        public bool IsSuccess { get; }
    }
}
