using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using AnBiaoZhiJianTong.Core.Contracts.Logging;
using AnBiaoZhiJianTong.Core.Contracts.Runtime;
using AnBiaoZhiJianTong.Core.Contracts.SQLite;
using AnBiaoZhiJianTong.Core.Contracts.SQLite.Mapper;
using AnBiaoZhiJianTong.Core.Events;
using AnBiaoZhiJianTong.Core.Services;
using AnBiaoZhiJianTong.Models;
using AnBiaoZhiJianTong.Shell.Models;
using AnBiaoZhiJianTong.Shell.ViewModels.Pages;
using AnBiaoZhiJianTong.Shell.Views;
using AnBiaoZhiJianTong.Shell.Views.CustomGeneralDialogs;
using AnBiaoZhiJianTong.Shell.Views.Pages;
using Newtonsoft.Json;
using Prism.Commands;
using Prism.DryIoc;
using Prism.Events;
using Prism.Ioc;
using Prism.Mvvm;
using SmartCheckOptimizer.Models;
using SqlSugar;


namespace AnBiaoZhiJianTong.Shell.ViewModels.Pages
{
    public enum ActionMode
    {
        一键优化,
        返回
    }

    public class CheckOutViewModel : BindableBase , IAcceptParameters
    {
        private readonly IDb3ContextProvider _provider;
        private readonly IAnBiaoDb3Repository _anBiaoDb3Repository;

        /// <summary>
        /// 检查状态
        /// </summary>
        private string _status;
        public string Status
        {
            get => _status;
            private set => SetProperty(ref _status, value);
        }

        /// <summary>
        /// 清标DLL的检查服务
        /// </summary>
        private readonly SmartCheckOptimizer.Contracts.IDocCheckAndOptimizeService _checkService;
        /// <summary>
        /// 清标DLL检查后生成的数据库文件路径
        /// </summary>
        private string _dbDirectory = string.Empty;
        /// <summary>
        /// 取消令牌源：用于中断当前正在执行的检查/优化异步任务。
        /// </summary>
        private CancellationTokenSource _cts;
        /// <summary>
        /// 是否正在检查
        /// </summary>
        private bool _isBusy;

        private bool IsBusy
        {
            get => _isBusy;
            set
            {
                if (!SetProperty(ref _isBusy, value)) return;
                // 忙碌时立刻显示遮罩层
                if (value)
                {
                    OverlayVisible = true;
                }
                // 忙碌转不忙碌 延迟 1.5 秒再隐藏遮罩层
                else
                {
                    Task.Delay(1500).ContinueWith(_ =>
                    {
                        Application.Current.Dispatcher.Invoke(() =>
                        {
                            OverlayVisible = false;
                        });
                    });
                }
            }
        }

        private ActionMode _mode = ActionMode.一键优化;
        private ActionMode Mode
        {
            get => _mode;
            set
            {
                if (SetProperty(ref _mode, value))
                {
                    RaisePropertyChanged(nameof(ButtonText));   // 切换按钮文字
                    DynamicCommand.RaiseCanExecuteChanged();    // 若 CanExecute 依赖 Mode，可保留
                }
            }
        }

        public string ButtonText => Mode == ActionMode.一键优化 ? "一键优化" : "返回";

        public DelegateCommand DynamicCommand { get; private set; }
        
        /// <summary>
        /// 等待遮罩显示
        /// </summary>
        private bool _overlayVisible;
        public bool OverlayVisible
        {
            get => _overlayVisible;
            set => SetProperty(ref _overlayVisible, value);
        }

        /// <summary>
        /// 判断按钮能不能点：只有不忙时才允许
        /// </summary>
        private bool CanExecute() => !IsBusy;

        private readonly ILogger _logger;
        private readonly IEventAggregator _eventAggregator;
        public ObservableCollection<MyTreeNode> RootItems { get; set; }
        private readonly IAppDataBus _appDataBus;
        public MyTreeNode SelectedItem { get; set; }

        private ResultItem _resultItem;
        public ResultItem ResultItem { get => _resultItem; set => SetProperty(ref _resultItem, value); }

        public CheckOutViewModel(IAppDataBus appDataBus, ILogger logger, IEventAggregator eventAggregator, IAnBiaoDb3Repository anBiaoDb3Repository, IDb3ContextProvider provider)
        {
            _provider = provider;
            _anBiaoDb3Repository = anBiaoDb3Repository;
            _appDataBus = appDataBus;
            _logger = logger;
            _eventAggregator = eventAggregator;
            _checkService = ((PrismApplication)Application.Current).Container
                    .Resolve<SmartCheckOptimizer.Contracts.IDocCheckAndOptimizeService>();

            DynamicCommand = new DelegateCommand(
                ExecuteByMode,
                CanExecute
                ).ObservesProperty(() => IsBusy); 
        }

        private void ExecuteByMode()
        {
            switch (Mode)
            {
                case ActionMode.一键优化:
                    ExecuteOptimizationButton();
                    break;
                case ActionMode.返回:
                    ExecuteReturnButton();
                    break;
            }
        }

        private void ExecuteReturnButton()
        {

        }


        public void ApplyParameters(MyNavigationParameters parameters)
        {
            ResultItem = parameters.Get<ResultItem>("Data");
            var documentCheckError = ResultItem.DocumentCheckErrors;
            var checkTypeResult = ResultItem.CheckTypeResults;

            var from = parameters.Get<string>("From");
            if (from.Equals("CheckResult"))
            {
                Mode = ActionMode.返回;
            }

            _appDataBus.TryGet("BaseAnBiaoFormat", out List<BaseAnBiaoFormatNode> baseAnBiaoFormatNodes);
            _appDataBus.TryGet("WordPath", out string path);
            List<MyTreeNodeData> treeNode =TreeBuilder.ConvertDocumentCheckErrorsToMyTreeNodeData(documentCheckError.ToList() ,checkTypeResult.ToList(), baseAnBiaoFormatNodes);
            _appDataBus.Set("TreeNode", treeNode);
            //RootItems = LeftBuildTree(treeNode);
            RootItems = new ObservableCollection<MyTreeNode>()
            {
                new MyTreeNode("",Path.GetFileName(path))
                {
                }
            };

        }
        /// <summary>
        /// 点击优化按钮时执行
        /// </summary>
        private async void ExecuteOptimizationButton()
        {
            try
            {
                // 第一步：执行优化
                var optimizedWordPath = await RunOptimizationAsync();
                _appDataBus.Set("OptimizedWordPath", optimizedWordPath);
                // 执行完优化后
                if (optimizedWordPath == null)
                {
                    _logger?.LogError($"清标优化出错:优化文档未能生成");
                    MessageBox.Show("优化失败：优化文档未能生成");
                }
                else
                {
                    // 获取数据库当前用户的最后一次操作记录
                    var lastOperationRecord = await _anBiaoDb3Repository.GetLastOperationRecordAsync(_appDataBus.Get<User>("User").UserId);
                    if (lastOperationRecord == null)
                        throw new InvalidOperationException("获取当前用户的最后一次操作记录为空");

                    // 跳转并携参


                    var db = _provider.GetDb();
                    var tran = await db.Ado.UseTranAsync(async () =>
                    {
                        // 更新 IsOptimized 列 和 OptimizedWordName 列
                        var rows = await db.Updateable<OperationRecord>()
                            .SetColumns(operationRecord => operationRecord.IsOptimized == true)
                            .SetColumns(operationRecord => operationRecord.OptimizedWordName == optimizedWordPath)
                            .Where(operationRecord => operationRecord.OperationId == lastOperationRecord.OperationId)
                            .ExecuteCommandAsync();

                        if (rows <= 0) throw new Exception("更新操作记录失败");
                    });
                    if (!tran.IsSuccess) throw tran.ErrorException ?? new Exception("数据库事务失败");
                }
            }
            catch (Exception ex)
            {
                // 兜底异常，防止 async void 异常被吞
                _logger?.LogError($"清标优化出错:{ex.Message}\r详情:{ex.Source}");
                var resp = CommonDialogWindow.Show(new CommonDialogOptions
                {
                    Title = "优化失败",
                    Header = "优化失败",
                    Message = "优化失败：" + ex.Message,
                    Icon = CommonDialogIcon.Success,
                    Owner = Application.Current?.MainWindow,
                    Buttons = new List<CommonDialogButton>
                        {
                            new CommonDialogButton { Text = "确认", Result = CommonDialogResult.Primary, IsDefault = true },
                        }
                });
            }
        }


        /// <summary>
        /// 预执行清标DLL的优化功能
        /// </summary>
        /// <returns></returns>
        private async Task<string> RunOptimizationAsync()
        {
            if (IsBusy) return null;
            _logger?.LogInfo($"====================开始清标优化====================");
            IsBusy = true;
            Status = "正在优化文档…";
            _cts = new CancellationTokenSource();
            try
            {
                var optimizedWordPath = await OptimizationAsync(_cts.Token);
                if (!string.IsNullOrEmpty(optimizedWordPath))
                {
                    Status = "即将优化完成";
                }
                return optimizedWordPath;
            }
            catch (OperationCanceledException)
            {
                Status = "已取消";
                throw;
            }
            catch (Exception ex)
            {
                Status = "优化失败";
                _logger?.LogError($"清标优化失败：{ex.Message}\r详情：{ex.Source}");
                var resp = CommonDialogWindow.Show(new CommonDialogOptions
                {
                    Title = "优化失败",
                    Header = "优化失败",
                    Message = "优化失败：" + ex.Message,
                    Icon = CommonDialogIcon.Success,
                    Owner = Application.Current?.MainWindow,
                    Buttons = new List<CommonDialogButton>
                        {
                            new CommonDialogButton { Text = "确认", Result = CommonDialogResult.Primary, IsDefault = true },
                        }
                });
                throw;
            }
            finally
            {
                IsBusy = false;
                _cts.Dispose();
                _cts = null;
            }
        }
        /// <summary>
        /// 清标DLL 优化文档
        /// </summary>
        /// <param name="token"></param>
        /// <returns></returns>
        private async Task<string> OptimizationAsync(CancellationToken token)
        {
            try
            {
                if (!(_appDataBus.TryGet("AnBiaoFormatPath", out string anBiaoFormatPath) &&
                      _appDataBus.TryGet("WordPath", out string wordPath)))
                {
                    var resp = CommonDialogWindow.Show(new CommonDialogOptions
                    {
                        Title = "未导入文件",
                        Header = "",
                        Message = "请导入标书文件并设置规则",
                        Icon = CommonDialogIcon.Success,
                        Owner = Application.Current?.MainWindow,
                        Buttons = new List<CommonDialogButton>
                        {
                            new CommonDialogButton { Text = "确认", Result = CommonDialogResult.Primary, IsDefault = true },
                        }
                    });
                    return null;
                }

                // 1. 加载格式规则json
                var formatJson = await Task.Run(() => File.ReadAllText(anBiaoFormatPath), token);
                var anBiaoFormatInfo = JsonConvert.DeserializeObject<AnBiaoFormatInfo>(formatJson);

                // 2. 组装请求参数
                var req = new DocumentOptimizeRequest
                {
                    XiaQuCode = "",
                    DocFilePath = wordPath,
                    FilePath = null,
                    AnBiaoFormatInfo = anBiaoFormatInfo,
                    DQArea = "",
                    ResultPath = _dbDirectory,
                    DanWeiName = null,
                    DanWeiNameLst = null,
                    PdfPath = null,
                    UnCheckPageNumLst = null
                };

                // 3. 调用服务
                var result = await Task.Run(() =>
                {
                    token.ThrowIfCancellationRequested();
                    var str = _checkService.OptimizeDocumentWithFormat(req);
                    return str;
                }, token);
                return result;
            }
            catch (Exception e)
            {
                _logger?.LogError($"清标优化DLL出错：{e.Message}\r详情：{e.Source}");
                return null;
            }
        }
    }

    public class TreeBuilder
    {
        //最左侧文件页面构建
        public static ObservableCollection<MyTreeNode> LeftBuildTree(List<MyTreeNodeData> data)
        {
            ObservableCollection<MyTreeNode> rootItems = new ObservableCollection<MyTreeNode>();
            if (data != null)
            {
                Dictionary<string, MyTreeNode> paragraphNodes = new Dictionary<string, MyTreeNode>();
                int i = 0;
                foreach (var item in data)
                {
                    i++;
                    item.Id = i;
                    // 如果 ErrorParagraph 还没有对应的 MyTreeNode，则创建一个新的节点
                    if (!paragraphNodes.ContainsKey(item.DocumentName))
                    {
                        MyTreeNode paragraphNode = new MyTreeNode(item.Id.ToString(), item.DocumentName)
                        {
                        };
                        paragraphNodes[item.ErrorParagraph] = paragraphNode;
                        rootItems.Add(paragraphNode);
                    }
                }
            }
            return rootItems;
        }
        //中间树状图错误项构建
        public static async Task<ObservableCollection<MyTreeNode>> MiddleFaleBuildTree(List<MyTreeNodeData> data)
        {

            var rootItems = new ObservableCollection<MyTreeNode>();
            if (data == null) return rootItems;
            var parentDict = new Dictionary<string, MyTreeNode>(); // Key: CheckPointTypeCode
            var predefinedNodes = new List<(string Code, string Title)>
            {
                ("全部项目", "全部项目"),
                ("内容检查", "内容检查"),
                ("封面检查", "封面"),
                ("目录检查", "目录"),
                ("标题检查", "标题"),
                ("正文检查", "正文"),
                ("段落检查", "段落"),
                ("图表检查", "图表"),
                ("页面检查", "页面"),
                ("其他检查", "其他检查"),
                ("人工检查项目","人工检查项目")
            };
            foreach (var node in predefinedNodes)
            {
                var parentNode = new MyTreeNode("", node.Title)
                {
                    Title = node.Code,
                    AssociatedDataItems = new ObservableCollection<MyTreeNodeData>()
                };
                parentDict[node.Title] = parentNode;
                rootItems.Add(parentNode);
            }
            var allItemsNode = parentDict["全部项目"];
            var HumancheckNode = parentDict["人工检查项目"];
            foreach (var item in data.Where(i => i.CheckResult == "错误" || i.CheckResult == "人工检查"))
            {
                allItemsNode.AssociatedDataItems.Add(item);
                // 1. 获取或创建父节点
                parentDict.TryGetValue(item.CheckPointTypeCode, out var parentNode);
                parentNode.AssociatedDataItems.Add(item);
                if(item.CheckResult == "错误" )
                {
                    parentNode.ErrorCount++;
                    allItemsNode.ErrorCount++;
                }
                else if(item.CheckResult == "人工检查")
                {
                    parentNode.ManualCount++;
                    allItemsNode.ManualCount++;
                    HumancheckNode.AssociatedDataItems.Add(item);
                    HumancheckNode.ManualCount++;
                }
                parentNode.TotalCount = parentNode.ErrorCount + parentNode.ManualCount;
                allItemsNode.TotalCount = allItemsNode.ErrorCount + allItemsNode.ManualCount;
                HumancheckNode.TotalCount = HumancheckNode.ErrorCount + HumancheckNode.ManualCount;
                // 2. 获取或创建子节点（按 CheckPointName 分组）
                //var childNode = parentNode.Children.FirstOrDefault(c => c.Title == item.CheckPointName);
                //if (childNode == null)
                //{
                //    childNode = new MyTreeNode("",item.CheckPointName)
                //    {
                //        Title = item.CheckPointName,
                //        AssociatedDataItems = new ObservableCollection<MyTreeNodeData>()
                //    };
                //    parentNode.Children.Add(childNode);
                //}
                //// 3. 将当前数据项关联到子节点
                //childNode.AssociatedDataItems.Add(item);
                //// 4. 更新子节点统计
                //if (item.CheckResult == "错误")
                //    childNode.ErrorCount++;
                //else if (item.CheckResult == "人工检查")
                //    childNode.ManualCount++;
                //childNode.TotalCount = childNode.ErrorCount + childNode.ManualCount;
                //// 5. 更新父节点统计（递归可选）
                //parentNode.ErrorCount = parentNode.Children.Sum(c => c.ErrorCount);
                //parentNode.ManualCount = parentNode.Children.Sum(c => c.ManualCount);
                //parentNode.TotalCount = parentNode.Children.Sum(c => c.TotalCount);
            }
            return rootItems;
        }
        //全部构建
        public static async Task<ObservableCollection<MyTreeNode>> MiddleAllBuildTree(List<MyTreeNodeData> data)
        {
            ObservableCollection<MyTreeNode> rootItems = new ObservableCollection<MyTreeNode>();

            if (data != null)
            {
                Dictionary<string, MyTreeNode> paragraphNodes = new Dictionary<string, MyTreeNode>();

                foreach (var item in data)
                {
                    // 如果 ErrorParagraph 还没有对应的 MyTreeNode，则创建一个新的节点
                    if (!paragraphNodes.ContainsKey(item.CheckPointTypeCode))
                    {
                        MyTreeNode paragraphNode = new MyTreeNode("", item.CheckPointTypeCode);
                        paragraphNodes[item.CheckPointTypeCode] = paragraphNode;
                        rootItems.Add(paragraphNode);
                    }
                    Dictionary<string, MyTreeNode> ChileNodes = new Dictionary<string, MyTreeNode>();
                    MyTreeNode newparagraphNode = paragraphNodes[item.CheckPointTypeCode];
                    // 创建子节点并添加到父节点的 Children 集合中
                    MyTreeNode childNode = new MyTreeNode("", item.CheckPointName); ;
                    if (!ChileNodes.ContainsKey(item.CheckPointName))
                    {
                        ChileNodes[item.CheckPointName] = childNode;
                        newparagraphNode.Children.Add(childNode);
                    }
                    Debug.WriteLine("Added child node with CheckPointName: " + item.CheckPointName + " to parent with CheckPointTypeCode: " + item.CheckPointTypeCode);
                }
            }
            Debug.WriteLine("MiddleAllBuildTree completed. rootItems count: " + rootItems?.Count);
            return rootItems;
        }
        //右侧页面内容构建

        //数据库字符转换
        public static MyTreeNodeData ConvertToMyTreeNodeData(DocumentCheckError documentCheckError, AnBiaoZhiJianTong.Models.CheckTypeResult checkTypeResult ,int id,Rule rules,SubCatalog    subCatalog)
        {
            if (documentCheckError == null)
            {
                throw new ArgumentNullException(nameof(documentCheckError));
            }
            if (checkTypeResult == null)
            {
                throw new ArgumentNullException(nameof(checkTypeResult));
            }

            // 检查 CheckPointType 是否与 CheckTypeResult.CheckType 一致
            if (documentCheckError.CheckPointType != checkTypeResult.CheckType)
            {
                // 如果不一致，可以选择忽略该条记录或抛出异常
                throw new ArgumentException("CheckPointType 不匹配 CheckTypeResult.CheckType");
            }
            string title = "";
            if(rules == null)
            {
                title = "其他检查";
            }
            else
            {
                title =subCatalog.CatalogName;
            }
            string result = "正确";
            Brush color = Brushes.Green;
            if (checkTypeResult.CheckResult == "1")
            {
                result = "正确";
            }
            else if (checkTypeResult.CheckResult == "2")
            {
                result = "错误";
                color = Brushes.Red;
            }
            else
            {
                result = "人工检查";
                color = Brushes.Orange;
            }
            return new MyTreeNodeData
            {
                Id = id,
                DocumentName = documentCheckError.DocumentName,
                ErrorPageNumber = documentCheckError.ErrorPageNumber,
                CheckPointName = documentCheckError.CheckPointName,
                CheckPointRequirement = documentCheckError.CheckPointRequirement,
                ErrorSummary = documentCheckError.ErrorSummary,
                ErrorDetail = documentCheckError.ErrorDetail,
                ErrorParagraph = documentCheckError.ErrorParagraph,
                ErrorRun = documentCheckError.ErrorRun,
                SectionIndex = documentCheckError.SectionIndex,
                CheckPointTypeCode =title ,
                CheckResult = result,
                TyprColor = color,
                NodeCode = documentCheckError.CheckPointTypeCode,
                Children = new List<MyTreeNodeData>()
            };
        }
        public static List<MyTreeNodeData> ConvertDocumentCheckErrorsToMyTreeNodeData(List<DocumentCheckError> data, List<AnBiaoZhiJianTong.Models.CheckTypeResult> checkTypeResults,List<BaseAnBiaoFormatNode> baseAnBiaoFormatNodes)
        {
            List<MyTreeNodeData> myTreeNodeDataList = new List<MyTreeNodeData>();
            int idCounter = 1; // 用于生成唯一的Id

            if (data != null && checkTypeResults != null)
            {
                // 使用字典来存储 CheckType 对应的 CheckTypeResult
                Dictionary<string, AnBiaoZhiJianTong.Models.CheckTypeResult> checkTypeDict = new Dictionary<string, AnBiaoZhiJianTong.Models.CheckTypeResult>();
                foreach (var checkTypeResult in checkTypeResults)
                {
                    checkTypeDict[checkTypeResult.CheckType] = checkTypeResult;
                }
                Dictionary<string, Rule> RuleDict = new Dictionary<string, Rule>();
                Dictionary<string, SubCatalog> subCatalogDict = new Dictionary<string, SubCatalog>();
                foreach (var baseAnBiaoFormatNode in baseAnBiaoFormatNodes)
                {
                    foreach (var subCatalog in baseAnBiaoFormatNode.SubCatalogs)
                    {
                       subCatalogDict[subCatalog.CatalogName] = subCatalog;
                        foreach(var rules in subCatalog.Rules)
                        {
                            RuleDict[rules.NodeCode] = rules;
                        }
                    }
                }
                // 遍历 DocumentCheckError 列表
                foreach (var documentCheckError in data)
                {
                    if (checkTypeDict.TryGetValue(documentCheckError.CheckPointType, out AnBiaoZhiJianTong.Models.CheckTypeResult checkTypeResult)&&RuleDict.TryGetValue(documentCheckError.CheckPointTypeCode,out Rule rule))
                    {
                        // 匹配成功，生成 MyTreeNodeData 并添加到列表中
                        try
                        {
                            var subCatalog = subCatalogDict.FirstOrDefault(x => x.Value.Rules.Any(r => r.NodeCode == documentCheckError.CheckPointTypeCode)).Value;
                            MyTreeNodeData treeNodeData = ConvertToMyTreeNodeData(documentCheckError, checkTypeResult, idCounter++, rule,subCatalog);
                            myTreeNodeDataList.Add(treeNodeData);
                        }
                        catch (ArgumentException ex)
                        {
                           MessageBox.Show("匹配数据错误:"+ex.Message);
                        }
                    }
                    else
                    {
                        MyTreeNodeData treeNodeData = ConvertToMyTreeNodeData(documentCheckError, checkTypeResult, idCounter++, null,null);
                        myTreeNodeDataList.Add(treeNodeData);
                    }
                }
            }
            return myTreeNodeDataList;
        }
    }
}

