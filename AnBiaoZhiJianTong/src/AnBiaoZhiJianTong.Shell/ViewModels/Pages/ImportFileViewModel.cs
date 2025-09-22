using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Configuration;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Input;
using AnBiaoZhiJianTong.Common.Utilities;
using AnBiaoZhiJianTong.Core.Contracts.Logging;
using AnBiaoZhiJianTong.Core.Contracts.Runtime;
using AnBiaoZhiJianTong.Core.Contracts.SQLite;
using AnBiaoZhiJianTong.Core.Contracts.SQLite.Mapper;
using AnBiaoZhiJianTong.Core.Events;
using AnBiaoZhiJianTong.Models;
using AnBiaoZhiJianTong.Shell.global;
using AnBiaoZhiJianTong.Shell.Models;
using AnBiaoZhiJianTong.Shell.Views.Windows;
using Microsoft.Win32;
using Prism.Commands;
using Prism.Events;
using Prism.Ioc;
using Prism.Mvvm;

namespace AnBiaoZhiJianTong.Shell.ViewModels.Pages
{
    public class ImportFileViewModel: BindableBase
    {
        public enum DisplayMode { Empty, FileSelected } // 选择文件时两种状态
        private DisplayMode _currentMode = DisplayMode.Empty;
        public DisplayMode CurrentMode
        {
            get => _currentMode;
            set => SetProperty(ref _currentMode, value);
        }


        private readonly ILogger _logger;
        private readonly IAppDataBus _appDataBus;
        private readonly IDb3ContextProvider _provider;
        private readonly IAnBiaoDb3Repository _anBiaoDb3Repository;
        private readonly IEventAggregator _eventAggregator;
        //导入投标文件状态
        private bool _isFileImported;
        public bool IsFileImported
        {
            get => _isFileImported;
            set
            {
                _isFileImported = value;
                CurrentMode = value ? DisplayMode.FileSelected : DisplayMode.Empty;
                RaisePropertyChanged(nameof(IsFileImported));
            }
        }
        private string _tenderFileTitle;
        public string TenderFileTitle
        {
            get => _tenderFileTitle;
            set => SetProperty(ref _tenderFileTitle, value);
        }
        /// <summary>
        /// 规则文件是否被选定
        /// </summary>
        private bool _isChecked;
        public bool IsChecked
        {
            get => _isChecked;
            set => SetProperty(ref _isChecked, value);
        }
        /// <summary>
        /// 标书文件的名
        /// </summary>
        private string _fileName;
        public string FileName
        {
            get => _fileName;
            set => SetProperty(ref _fileName, value);
        }
        private ObservableCollection<CheckItem> _checkItems = new ObservableCollection<CheckItem>();
        public ObservableCollection<CheckItem> CheckItems
        {
            get => _checkItems;
            private set => SetProperty(ref _checkItems, value);
        }
        // 使用 ObservableCollection 代替 List
        private ObservableCollection<ImportedFileInfo> _importedFiles = new ObservableCollection<ImportedFileInfo>();
        public ObservableCollection<ImportedFileInfo> ImportedFiles
        {
            get { return _importedFiles; }
            set { SetProperty(ref _importedFiles, value); }
        }
        /// <summary>
        /// 规则文件导入模板
        /// </summary>
        private ObservableCollection<TemplateModel> _templates;
        public ObservableCollection<TemplateModel> Templates
        {
            get => _templates;
            set => SetProperty(ref _templates, value);
        }
        /// <summary>
        /// 被选中的规则文件
        /// </summary>
        private TemplateModel _selectedTemplate;
        public TemplateModel SelectedTemplate
        {
            get => _selectedTemplate;
            set => SetProperty(ref _selectedTemplate, value);
        }
        private bool _isManualImport;
        public bool IsManualImport
        {
            get => _isManualImport;
            set
            {
                SetProperty(ref _isManualImport, value);
                // 当手动导入状态改变时，重置地区和模板选择
                if (!value)
                {
                    SelectedRegion = null;
                    SelectedCheckRule = null;
                }
            }
        }
        private string _selectedRegion;
        public string SelectedRegion
        {
            get => _selectedRegion;
            set
            {
                SetProperty(ref _selectedRegion, value);
            }
        }
        private string _selectedCheckRule;
        public string SelectedCheckRule
        {
            get => _selectedCheckRule;
            set
            {
                SetProperty(ref _selectedCheckRule, value);
            }
        }
        private readonly string _baseAnBiaoFormatName = KeyEncryption.DecryptAes(ConfigurationManager.AppSettings["BaseAnBiaoFormatName"]); //BaseAnBiaoFormat json文件名
        private const string AnBiaoFormatName = "anbiaoformat.json"; // AnBiaoFormat json文件名
        private const string OtherCheckName = "其它检查"; // BaseAnBiaoFormat中的"其它检查"目录名
        /// <summary>
        /// 规则文件输出路径
        /// </summary>
        private string OutPath;
        /// <summary>
        /// 全系规则
        /// </summary>
        private List<BaseAnBiaoFormatNode> _baseAnBiaoFormat = new List<BaseAnBiaoFormatNode>();
        /// <summary>
        /// 导出的规则
        /// </summary>
        private AnBiaoFormat _anBiaoFormat = new AnBiaoFormat();
        /// <summary>
        /// 全系规则匹配结果(根据nodeCode匹配后在全系规则模型基础上加上MatchValue字段)
        /// </summary>
        private List<MatchAnBiaoFormatNode> _matchAnBiaoFormat;
        /// <summary>
        /// 需要发布的字段(订阅者只读)
        /// </summary>
        public IReadOnlyList<MatchAnBiaoFormatNode> CurrentMatched => _matchAnBiaoFormat;
        /// <summary>
        /// 导入投标文件
        /// </summary>
        public DelegateCommand ImportFileCommand { get; private set; }
        public DelegateCommand<ImportedFileInfo> DeleteFileCommand { get; private set; }
        /// <summary>
        /// 支付命令
        /// </summary>
        public DelegateCommand AffordCommand { get; private set; }
        /// <summary>
        /// 导入招标规则文件
        /// </summary>
        public DelegateCommand ImportTenderCommand { get; private set; }
        /// <summary>
        /// 右侧编辑按钮命令
        /// </summary>
        public DelegateCommand<CheckItem> EditCommand { get; }
        /// <summary>
        /// 查看对应文件
        /// </summary>
        public DelegateCommand<TemplateModel> ViewCommand { get; }
        /// <summary>
        /// 删除对应文件
        /// </summary>
        public DelegateCommand<TemplateModel> DeleteCommand { get; }
        /// <summary>
        /// 重命名对应文件
        /// </summary>
        public DelegateCommand<TemplateModel> RenameCommand { get; }
        public ImportFileViewModel(IAppDataBus appDataBus, ILogger logger, IDb3ContextProvider provider, IAnBiaoDb3Repository anBiaoDb3Repository, IEventAggregator eventAggregator)
        {
            _logger = logger;
            _provider = provider;
            _anBiaoDb3Repository = anBiaoDb3Repository;
            _appDataBus = appDataBus;
            _eventAggregator = eventAggregator;
            Templates = new ObservableCollection<TemplateModel>();
            ImportFile();
            ImportFileCommand = new DelegateCommand(async () => await ImportFileWindow());
            DeleteFileCommand = new DelegateCommand<ImportedFileInfo>(DeleteFile);
            AffordCommand = new DelegateCommand(Afford);
            EditCommand = new DelegateCommand<CheckItem>(OnEdit, CanEdit);
            ImportTenderCommand = new DelegateCommand(async () => await ImportTender());
            ViewCommand = new DelegateCommand<TemplateModel>(OnView);
            DeleteCommand = new DelegateCommand<TemplateModel>(OnDelete);
            RenameCommand = new DelegateCommand<TemplateModel>(OnRename);
        }
        private void ImportFile()
        {
            _appDataBus.TryGet("ImportedFiles", out ObservableCollection<ImportedFileInfo> importedFiles);
            if (importedFiles != null)
            {
                ImportedFiles = importedFiles;
            }
        }

        /// <summary>
        /// 投标文件导入
        /// </summary>
        private async Task ImportFileWindow()
        {
            // 使用 Microsoft.Win32.OpenFileDialog
            OpenFileDialog openFileDialog = new OpenFileDialog
            {
                Filter = "Word文档|*.docx|所有文件|*.*",
                Title = "导入技术标文档"
            };

            if (openFileDialog.ShowDialog() != true) return;

            // 如果已存在文档，清除原有文档
            if (ImportedFiles.Any())
            {
                ImportedFiles.Clear();
            }

            string targetDir = null;
            string pdfPath = null;
            string wordPath = null;

            // Word转PDF 供清标使用
            try
            {
                // 1) 拷贝到指定文件夹
                targetDir = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "AppData", "ImportedFiles", Guid.NewGuid().ToString());
                var copyResult = await FunctionHelper.CopyFileToTargetDirAsync(sourcePath: openFileDialog.FileName, targetDir: targetDir);
                if (!copyResult.Success)
                    throw new InvalidOperationException($"导入文件失败：{copyResult.ErrorMessage}");
                wordPath = copyResult.TargetPath;

                // 2) 导入文档
                var fileInfo = new FileInfo(wordPath);
                ImportedFiles.Add(new ImportedFileInfo
                {
                    Index = 1,
                    FileName = fileInfo.Name,
                    FilePath = fileInfo.FullName,
                    FileSize = fileInfo.Length,
                    ImportTime = DateTime.Now
                });
                FileName =   fileInfo.Name;
                IsFileImported = true;
                // 存储导入的文档信息到AppDataBus
                _appDataBus.Set("ImportedFiles", ImportedFiles);
                _appDataBus.Set("Imported", ImportedFiles.Any());

                // 3) Word转PDF
                pdfPath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "Output", "Word2Pdf", Guid.NewGuid().ToString(), $"{Path.GetFileNameWithoutExtension(fileInfo.Name)}.pdf");
                var word2Pdf = new Word2Pdf(openFileDialog.FileName, pdfPath);
                word2Pdf.Convert();

                // 4) 存储操作步骤到数据库（事务）
                //var operationRecord = new OperationRecord
                //{
                //    OperationId = 0,
                //    UserId = _appDataBus.Get<User>("User").UserId,
                //    OperationWordName = copyResult.TargetPath,
                //    OperationRuleName = null,
                //    OperationPdfName = pdfPath,
                //    AnBiaoFormatPath = null,
                //    IsChecked = false,
                //    IsOptimized = false,
                //    OptimizedWordName = null,
                //    IsExported = false,
                //    User = null
                //};
                //var db = _provider.GetDb();                         // ← 通过你现有的 provider 拿 SqlSugarClient
                //var tran = await db.Ado.UseTranAsync(async () =>     // ← 原子化：这里面若抛异常，事务会回滚
                //{
                //    // 可以是多条写库操作，全部在同一事务里
                //    var ok = await _anBiaoDb3Repository.SaveAbleOperationRecordAsync(operationRecord);
                //    if (!ok) throw new Exception("保存操作记录失败");
                //});
                //if (!tran.IsSuccess) throw tran.ErrorException ?? new Exception("数据库事务失败");

                // 事务成功后再写入路径，供后续使用
                _logger?.LogInfo($"【AppDataBus】存储导入的WORD和转换的PDF路径:WordPath && PdfPath");
                _appDataBus.Set("WordPath", copyResult.TargetPath);
                _appDataBus.Set("PdfPath", pdfPath);
            }
            catch (Exception e)
            {
                // 回滚文件和内存状态
                if (!string.IsNullOrEmpty(wordPath) && File.Exists(wordPath))
                {
                    try { File.Delete(wordPath); }
                    catch
                    {
                        // ignored
                    }
                }
                if (!string.IsNullOrEmpty(targetDir) && Directory.Exists(targetDir))
                {
                    try { Directory.Delete(targetDir, true); }
                    catch
                    {
                        // ignored
                    }
                }
                if (!string.IsNullOrEmpty(pdfPath) && File.Exists(pdfPath))
                {
                    try { File.Delete(pdfPath); }
                    catch
                    {
                        // ignored
                    }
                }
                ImportedFiles.Clear();
                _appDataBus.Remove("ImportedFiles");
                _appDataBus.Remove("Imported");
                _appDataBus.Remove("WordPath");
                _appDataBus.Remove("PdfPath");

                _logger?.LogError($"Word转PDF失败：{e.Message}\r详情:{e.Source}");
                MessageBox.Show($"导入失败：{e.Message}", "错误", MessageBoxButton.OK, MessageBoxImage.Error);
                throw;
            }
        }
        private async void DeleteFile(ImportedFileInfo fileInfo)
        {
            ImportedFiles.Remove(fileInfo);
            for (int i = 0; i < ImportedFiles.Count; i++)
            {
                ImportedFiles[i].Index = i + 1;
            }
            IsFileImported = false;
            //var lastOperationRecord = await _anBiaoDb3Repository.GetLastOperationRecordAsync(_appDataBus.Get<User>("User").UserId);
            //if (lastOperationRecord == null)
            //    throw new InvalidOperationException("获取当前用户的最后一次操作记录为空");
            //var db = _provider.GetDb();
            //var tran = await db.Ado.UseTranAsync(async () =>
            //{
            //    // 更新 OperationWordName列 为空
            //    var rows = await db.Updateable<OperationRecord>()
            //        .SetColumns(operationRecord => operationRecord.OperationWordName == null)
            //        .Where(operationRecord => operationRecord.OperationId == lastOperationRecord.OperationId)
            //        .ExecuteCommandAsync();

            //    if (rows <= 0) throw new Exception("更新操作记录失败");
            //});
            //if (!tran.IsSuccess) throw tran.ErrorException ?? new Exception("数据库事务失败");
        }
        /// <summary>
        /// 支付命令
        /// </summary>
        private async void Afford()
        {
            PayWindow payWindow = new PayWindow();
            bool result = await PayWindow._payUrlViewModel.UserPayCheck();
            if (!result)
            {
                payWindow.Show();

                while (!result)
                {
                    result = await PayWindow._payUrlViewModel.UserPayCheck();
                    if (payWindow.IsClosed)
                    {
                        return;
                    }
                    await Task.Delay(1000);
                }
                //UpdateCheck = await PayWindow._payUrlViewModel.UpadataCheckResult();
                payWindow.Close();
            }
            else
            {
                //UpdateCheck = await PayWindow._payUrlViewModel.UpadataCheckResult();
            }
            //页面跳转
        }
        private void OnEdit(CheckItem item)
        {
            if (item == null) return;
            // TODO: 弹窗/右侧面板编辑 item
        }

        private bool CanEdit(CheckItem item)
        {
            return item != null; // 需要更细规则可在此控制
        }
        /// <summary>
        /// 预览
        /// </summary>
        private void OnView(TemplateModel item)
        {
            if (item == null) return;
            // 打开预览页面
        }
        /// <summary>
        /// 删除
        /// </summary>
        /// <param name="item"></param>
        private void OnDelete(TemplateModel item)
        {
            if (item == null) return;
            Templates.Remove(item);
        }
        /// <summary>
        /// 重命名
        /// </summary>
        private void OnRename(TemplateModel item)
        {
            if (item == null) return;
            var newName = ShowRenameDialogAndReturnNewName(item);
            if (!string.IsNullOrWhiteSpace(newName))
                item.Name = newName.Trim();

        }
        /// <summary>
        /// 获取新名字弹窗
        /// </summary>
        /// <param name="t"></param>
        /// <returns></returns>
        private string ShowRenameDialogAndReturnNewName(TemplateModel t)
        {
            var renameWindow = new ShowChangeName();
            if (renameWindow.ShowDialog() == true)
            {
                string newName = renameWindow.EnteredName;
                if (newName != null)
                {
                    return newName;
                }
            }
            return t.Name;
        }
        /// <summary>
        /// 招标文件导入
        /// </summary>
        private async Task ImportTender()
        {
            string targetDir = null;
            string tenderPath = null;
            var openFileDialog = new Microsoft.Win32.OpenFileDialog
            {
                Filter = "标书文件 (*.QHZF)|*.QHZF"
            };
            if (openFileDialog.ShowDialog() != true) return;
            IsManualImport = true;
            // 复制文件到AppData/TenderFiles
            targetDir = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "AppData", "TenderFiles", Guid.NewGuid().ToString());
            var copyPath = await FunctionHelper.CopyFileToTargetDirAsync(sourcePath: openFileDialog.FileName, targetDir: targetDir);
            if (!copyPath.Success)//规则文件规格失败
                throw new InvalidOperationException($"导入文件失败：{copyPath.ErrorMessage}");
            tenderPath = copyPath.TargetPath;
            var fileInfo = new FileInfo(tenderPath);
            TenderFileTitle = fileInfo.Name;
            TemplateModel template = new TemplateModel
            {
                Name = TenderFileTitle,
            };
            Templates.Add(template);
            OutPath = AnalyzeDocumentAsync(tenderPath);
            if (OutPath == null) return;
            var outAnBiaoFormatPath = Path.GetDirectoryName(OutPath);
            if (outAnBiaoFormatPath == null) return;
            var anBiaoFormatPath = Path.Combine(outAnBiaoFormatPath, AnBiaoFormatName);
            //更新数据库
            //var lastOperationRecord = await _anBiaoDb3Repository.GetLastOperationRecordAsync(_appDataBus.Get<User>("User").UserId);
            //if (lastOperationRecord == null)
            //    throw new InvalidOperationException("获取当前用户的最后一次操作记录为空");
            //var db = _provider.GetDb();
            //var tran = await db.Ado.UseTranAsync(async () =>
            //{
            //    // 更新 IsChecked、OperationRuleName和AnBiaoFormatPath 列
            //    var rows = await db.Updateable<OperationRecord>()
            //        .SetColumns(operationRecord => operationRecord.OperationRuleName == tenderPath)
            //        .SetColumns(operationRecord => operationRecord.AnBiaoFormatPath == anBiaoFormatPath)
            //        .Where(operationRecord => operationRecord.OperationId == lastOperationRecord.OperationId)
            //        .ExecuteCommandAsync();

            //    if (rows <= 0) throw new Exception("更新操作记录失败");
            //});
            //if (!tran.IsSuccess) throw tran.ErrorException ?? new Exception("数据库事务失败");
            _logger?.LogInfo($"【AppDataBus】存储导出规则Json文件路径:anBiaoFormatPath");
            _appDataBus.Set("AnBiaoFormatPath", anBiaoFormatPath);
            _appDataBus.Set("TenderPath", copyPath.TargetPath);
            _ = InitializeImportAsync(anBiaoFormatPath);
        }
        /// <summary>
        /// 接口库处理文件路径
        /// </summary>
        /// <param name="filePath"></param>
        /// <returns></returns>
        private string AnalyzeDocumentAsync(string filePath)
        {
            try
            {
                CheckBSManage checkBsManage = new CheckBSManage();

                if (!string.IsNullOrEmpty(filePath))
                {
                    OutPath = checkBsManage.SeparateZB(filePath, out _);
                    return OutPath;
                }
                else
                {
                    MessageBox.Show("路径获取错误：", "错误", MessageBoxButton.OK, MessageBoxImage.Error);
                    return "False";
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show("调用接口库时出错: " + ex.Message, "错误", MessageBoxButton.OK, MessageBoxImage.Error);
                return null;
            }
        }

        /// <summary>
        /// 加载规则数据，匹配规则数据，构建表格数据
        /// </summary>
        /// <param name="anBiaoFormatPath"></param>
        /// <returns></returns>
        private async Task InitializeImportAsync(string anBiaoFormatPath)
        {
            var baseAnBiaoFormatPath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "configs", _baseAnBiaoFormatName);
            _baseAnBiaoFormat = await Common.Utilities.FunctionHelper.Json2ModelAsync<List<BaseAnBiaoFormatNode>>(baseAnBiaoFormatPath);
            // todo:这里获取到
            _logger?.LogInfo($"【AppDataBus】存储全系规则Json:_baseAnBiaoFormat");
            _appDataBus.Set("BaseAnBiaoFormat", _baseAnBiaoFormat);
            _anBiaoFormat = await Common.Utilities.FunctionHelper.Json2ModelAsync<AnBiaoFormat>(anBiaoFormatPath);
            _matchAnBiaoFormat = MatchRule();

            // 构造第一列数据
            BuildInitialRows();
            // 重构matchAnBiaoFormat，排除MatchRule中MatchValue为空字符串的项
            var filterMatchAnBiaoFormat = FilterEmptyMatchValue(_matchAnBiaoFormat);
            // 构建第二列数据
            BuildImportRows(filterMatchAnBiaoFormat);
        }

        /// <summary>
        /// 规则编号匹配
        /// </summary>
        /// <param name="nodeCode">规则编号</param>
        /// <returns>映射后的节点编号</returns>
        private string AutoNodeCode(string nodeCode)
        {
            switch (nodeCode)
            {
                case "B0504": return "B0804";
                case "B0505": return "B0805";
                case "B0501": return "B0801";
                case "B0603": return "B0814";
                case "B0604": return "B0815";
                case "B0605": return "B0816";
                case "B0602": return "B0813";
                case "B0506": return "B0806";
                case "B0503": return "B0803";
                case "B0502": return "B0802";
                default: return nodeCode;
            }
        }

        /// <summary>
        /// 匹配解析各检查项下被选择的检查点
        /// </summary>
        private List<MatchAnBiaoFormatNode> MatchRule()
        {
            List<MatchAnBiaoFormatNode> matchAnBiaoFormat = new List<MatchAnBiaoFormatNode>();

            if (_baseAnBiaoFormat == null || _baseAnBiaoFormat.Count <= 0) return null;

            var sites = _anBiaoFormat?.AnBiaoRoot?.Sites;

            if (sites == null || sites.Count == 0) return null;

            Dictionary<string, Node> codeToValue = new Dictionary<string, Node>(StringComparer.OrdinalIgnoreCase);
            foreach (var site in sites)
            {
                foreach (var node in site.Nodes ?? Enumerable.Empty<Node>())
                {
                    if (node == null) continue;
                    var code = AutoNodeCode(node.NodeCode);
                    if (string.IsNullOrWhiteSpace(code)) continue;
                    codeToValue[code] = node;
                }
            }

            // 以_baseAnBiaoFormat为基准，统计被选中的检查点，从而构造List<MatchAnBiaoFormatNode>，MatchRule字段MatchValue的值为_AnBiaoFormat的中对应NodeCode的Node的NodeValue
            foreach (var baseCat in _baseAnBiaoFormat)
            {
                if (baseCat == null) continue;

                var matchCat = new MatchAnBiaoFormatNode
                {
                    CatalogName = baseCat.CatalogName,
                    SubCatalogs = new List<MatchSubCatalog>()
                };

                foreach (var sub in baseCat.SubCatalogs ?? new List<SubCatalog>())
                {
                    var matchSub = new MatchSubCatalog
                    {
                        CatalogName = sub?.CatalogName ?? string.Empty,
                        Rules = new List<MatchRule>()
                    };

                    foreach (var rule in sub?.Rules ?? new List<Rule>())
                    {
                        if (rule == null) continue;

                        string matchValue = string.Empty;
                        if (!string.IsNullOrWhiteSpace(rule.NodeCode) && codeToValue.TryGetValue(rule.NodeCode, out var mNode))
                        {
                            // 匹配成功后，从 codeToValue 删除，避免重复 & 便于后续统计未匹配项 
                            codeToValue.Remove(rule.NodeCode);

                            matchValue = mNode.NodeValue ?? string.Empty;
                        }
                        matchSub.Rules.Add(new MatchRule
                        {
                            NodeCode = rule.NodeCode,
                            NodeName = rule.NodeName,
                            NodeValue = rule.NodeValue,
                            NodeUnit = rule.NodeUnit,
                            Type = rule.Type,
                            CheckRequirementItem = rule.CheckRequirementItem,
                            IsRenGong = rule.IsRenGong,
                            IsParent = rule.IsParent,
                            AttriBute = rule.AttriBute,
                            NodeInfo = rule.NodeInfo,
                            MatchValue = matchValue
                        });
                    }

                    matchCat.SubCatalogs.Add(matchSub);
                }

                matchAnBiaoFormat.Add(matchCat);
            }

            // 未匹配的检查点，添加到"其它检查"目录下
            MatchSubCatalog otherCatalog = matchAnBiaoFormat
                .SelectMany(cat => cat.SubCatalogs)
                .FirstOrDefault(sub => string.Equals(sub.CatalogName, OtherCheckName, StringComparison.OrdinalIgnoreCase));

            if (otherCatalog == null) return matchAnBiaoFormat;

            // 补充_anBiaoFormat 中未匹配在 baseAnBiaoFormat的节点到"其他" 内容
            foreach (var otherCodeToValue in codeToValue)
            {
                if (otherCodeToValue.Value == null) continue;

                otherCatalog.Rules.Add(new MatchRule
                {
                    NodeCode = otherCodeToValue.Key,
                    NodeName = otherCodeToValue.Value.NodeName ?? otherCodeToValue.Key,     // 优先用 AnBiaoFormat 的 NodeName
                    NodeValue = string.Empty,                                               // 基准未定义，保持空
                    NodeUnit = string.Empty,
                    Type = nameof(RuleType.判定),                                            // "其它检查"里都是checkBox控件
                    CheckRequirementItem = string.Empty,
                    IsRenGong = otherCodeToValue.Value.IsRenGong ?? "否",                   // 用 AnBiaoFormat 的 isrengong
                    IsParent = string.Empty,
                    AttriBute = string.Empty,
                    NodeInfo = string.Empty,
                    MatchValue = otherCodeToValue.Value.NodeValue ?? string.Empty
                });
            }

            return matchAnBiaoFormat;
        }

        /// <summary>
        /// 重构matchAnBiaoFormat，排除MatchRule中MatchValue为空字符串的项
        /// </summary>
        /// <param name="source"></param>
        /// <param name="pruneEmptyContainers">true：连带删除空的子目录/目录；false：仅删除规则项</param>
        /// <returns></returns>
        private static List<MatchAnBiaoFormatNode> FilterEmptyMatchValue(IEnumerable<MatchAnBiaoFormatNode> source, bool pruneEmptyContainers = false)
        {
            var result = new List<MatchAnBiaoFormatNode>();
            if (source == null) return result;

            foreach (var cat in source)
            {
                if (cat == null) continue;

                var newCat = new MatchAnBiaoFormatNode
                {
                    CatalogName = cat.CatalogName,
                    SubCatalogs = new List<MatchSubCatalog>()
                };

                foreach (var sub in cat.SubCatalogs ?? Enumerable.Empty<MatchSubCatalog>())
                {
                    var filteredRules = (sub.Rules ?? new List<MatchRule>())
                        .Where(r => r != null && !string.IsNullOrEmpty(r.MatchValue)) // 只排除空字符串；如需把空白也排除，改为 IsNullOrWhiteSpace
                        .ToList();

                    if (filteredRules.Count == 0 && pruneEmptyContainers) continue;

                    newCat.SubCatalogs.Add(new MatchSubCatalog
                    {
                        CatalogName = sub.CatalogName,
                        Rules = filteredRules
                    });
                }

                if (newCat.SubCatalogs.Count == 0 && pruneEmptyContainers) continue;

                result.Add(newCat);
            }

            return result;
        }

        /// <summary>
        /// 第一列有值，第二列为空
        /// </summary>
        private void BuildInitialRows()
        {
            CheckItems.Clear();
            if (_baseAnBiaoFormat == null) return;

            foreach (var baseAnBiaoFormatNode in _baseAnBiaoFormat)
            {
                if (baseAnBiaoFormatNode?.SubCatalogs == null) continue;

                foreach (var sub in baseAnBiaoFormatNode.SubCatalogs)
                {
                    CheckItems.Add(new CheckItem
                    {
                        ItemName = sub?.CatalogName ?? string.Empty, // 第一列检查项数据
                        CheckContent = string.Empty,                 // 第二列先空
                        IsHeader = !false,
                        BackgroundColor = "Transparent"
                    });
                }
            }
            _appDataBus.Set("CheckItem", CheckItems);
        }

        /// <summary>
        /// 第二列数据构建
        /// </summary>
        /// <param name="filterMatchAnBiaoFormat">匹配后的除空的规则集合</param>
        private void BuildImportRows(List<MatchAnBiaoFormatNode> filterMatchAnBiaoFormat)
        {
            if (filterMatchAnBiaoFormat == null || filterMatchAnBiaoFormat.Count == 0 || CheckItems == null)
                return;

            // 以行的“检查项(=二级目录名)”建立索引（与 BuildInitialRows 的 ItemName 对应）
            var rowMap = CheckItems
                .Where(r => r != null && !string.IsNullOrWhiteSpace(r.ItemName))
                .GroupBy(r => r.ItemName, StringComparer.OrdinalIgnoreCase)
                .ToDictionary(g => g.Key, g => g.First(), StringComparer.OrdinalIgnoreCase);

            foreach (var cat in filterMatchAnBiaoFormat)
            {
                foreach (var sub in cat?.SubCatalogs ?? Enumerable.Empty<MatchSubCatalog>())
                {
                    if (sub?.Rules == null) continue;

                    var parts = new List<string>(sub.Rules.Count);
                    foreach (var rule in sub.Rules)
                    {
                        if (rule == null) continue;

                        var nodeName = rule.NodeName?.Trim();
                        var matchValue = rule.MatchValue?.Trim();
                        if (string.IsNullOrEmpty(nodeName) || string.IsNullOrEmpty(matchValue)) continue;

                        var keyName = Common.Utilities.FunctionHelper.ExtractDisplayValue(matchValue, "keyname");

                        // Todo: 数字类补单位，在此判断：
                        /*if (string.IsNullOrEmpty(rule.NodeUnit) == false && char.IsDigit(keyName.FirstOrDefault()))
                            keyName = $"{keyName}{rule.NodeUnit}";*/

                        parts.Add($"【{nodeName}：{keyName}】");
                    }

                    // 如果 parts 为空，则显示 “XXX不做要求”
                    var content = parts.Count > 0
                       ? string.Join("", parts)
                        : $"【{sub.CatalogName}不做要求】";

                    if (!string.IsNullOrWhiteSpace(sub.CatalogName) &&
                        rowMap.TryGetValue(sub.CatalogName, out var row))
                    {
                        row.CheckContent = content;          // 触发通知 → 第二列刷新
                        row.BackgroundColor = "Transparent"; // 触发通知 → 背景色刷新
                    }
                }
            }
        }
    }
}
