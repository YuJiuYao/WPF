using System;
using System.Collections.ObjectModel;
using AnBiaoZhiJianTong.Core.Contracts.Configuration;
using AnBiaoZhiJianTong.Core.Contracts.Http.Auth;
using AnBiaoZhiJianTong.Core.Events;
using AnBiaoZhiJianTong.Models.LoginDTO;
using HandyControl.Data;
using Prism.Events;
using Prism.Mvvm;

namespace AnBiaoZhiJianTong.Shell.ViewModels
{
    /// <summary>
    /// 主窗口视图模型，负责提供窗口布局和流程步骤展示数据。
    /// </summary>
    public class MainWindowViewModel : BindableBase
    {
        private readonly IAppConfiguration _configuration;
        private readonly IAuthService _authService;
        private readonly IEventAggregator _eventAggregator;

        /// <summary>
        /// 当前步骤索引。
        /// </summary>
        private int _currentStepIndex;
        public int CurrentStepIndex
        {
            get => _currentStepIndex;
            set => SetProperty(ref _currentStepIndex, value);
        }
        private bool _isUserAuthenticated;
        public bool IsUserAuthenticated
        {
            get => _isUserAuthenticated;
            private set
            {
                if (SetProperty(ref _isUserAuthenticated, value))
                {
                    RaisePropertyChanged(nameof(IsUserUnauthenticated));
                }
            }
        }

        public bool IsUserUnauthenticated => !IsUserAuthenticated;
        public MainWindowViewModel(IAppConfiguration configuration, IAuthService authService, IEventAggregator eventAggregator)
        {
            _configuration = configuration ?? throw new ArgumentNullException(nameof(configuration));
            _authService = authService ?? throw new ArgumentNullException(nameof(authService));
            _eventAggregator = eventAggregator ?? throw new ArgumentNullException(nameof(eventAggregator));

            Steps = new ObservableCollection<StepBarItemViewModel>
            {
                new StepBarItemViewModel("准备", "初始化环境"),
                new StepBarItemViewModel("执行1", "运行主要流程1"),
                new StepBarItemViewModel("执行2", "运行主要流程2"),
                new StepBarItemViewModel("执行3", "运行主要流程3"),
                new StepBarItemViewModel("执行4", "运行主要流程4"),
                new StepBarItemViewModel("完成", "流程结束")
            };

            _currentStepIndex = 0;

            UpdateAuthenticationState(_authService.IsAuthenticated);

            _eventAggregator
                .GetEvent<LoginInfoEvent>()
                .Subscribe(OnLoginInfoChanged, ThreadOption.UIThread);
        }

        /// <summary>
        /// 窗口标题。
        /// </summary>
        public string WindowTitle => $"{_configuration.AppName}";
        public string WindowVersion => $"{_configuration.AppVersion}";


        /// <summary>
        /// StepBar 的步骤集合。
        /// </summary>
        public ObservableCollection<StepBarItemViewModel> Steps { get; }

        /// <summary>
        /// 用于窗口尺寸设置的原始配置。
        /// </summary>
        public WindowSettings WindowLayout => _configuration.WindowSettings;

        private void OnLoginInfoChanged(LogininApiResponse info)
        {
            UpdateAuthenticationState(info != null);
        }

        private void UpdateAuthenticationState(bool isAuthenticated)
        {
            IsUserAuthenticated = isAuthenticated;
        }
    }

    /// <summary>
    /// StepBar 项目的视图模型。
    /// </summary>
    public class StepBarItemViewModel : BindableBase
    {
        public StepBarItemViewModel(string title, string description, StepStatus status = StepStatus.Waiting)
        {
            Title = title;
            Description = description;
            _status = status;
        }

        /// <summary>
        /// 步骤标题。
        /// </summary>
        public string Title { get; }

        /// <summary>
        /// 步骤描述。
        /// </summary>
        public string Description { get; }

        private StepStatus _status;
        public StepStatus Status
        {
            get => _status;
            set => SetProperty(ref _status, value);
        }
    }
}
