using System.Collections.ObjectModel;
using AnBiaoZhiJianTong.Models;
using Prism.Mvvm;

namespace AnBiaoZhiJianTong.Shell.Models
{
    public class ResultItem : BindableBase
    {
        private string _fileIndex;
        public string FileIndex
        {
            get => _fileIndex;
            set => SetProperty(ref _fileIndex, value);
        }

        private string _fileName;
        public string FileName
        {
            get => _fileName;
            set => SetProperty(ref _fileName, value);
        }

        private string _filePath;
        public string FilePath
        {
            get => _filePath;
            set => SetProperty(ref _filePath, value);
        }

        /// <summary>
        /// 表DocumentCheckError内容
        /// </summary>
        private ObservableCollection<DocumentCheckError> _documentCheckErrors;
        public ObservableCollection<DocumentCheckError> DocumentCheckErrors
        {
            get => _documentCheckErrors;
            set => SetProperty(ref _documentCheckErrors, value);
        }

        /// <summary>
        /// 表CheckTypeResult内容
        /// </summary>
        private ObservableCollection<CheckTypeResult> _checkTypeResults;
        public ObservableCollection<CheckTypeResult> CheckTypeResults
        {
            get => _checkTypeResults;
            set => SetProperty(ref _checkTypeResults, value);
        }
    }


    public class CheckSummary : BindableBase
    {
        private bool _isVisible;
        public bool IsVisible
        {
            get => _isVisible;
            set => SetProperty(ref _isVisible, value);
        }

        private int _totalFiles;
        public int TotalFiles
        {
            get => _totalFiles;
            set => SetProperty(ref _totalFiles, value);
        }

        private int _failedFiles;
        public int FailedFiles
        {
            get => _failedFiles;
            set => SetProperty(ref _failedFiles, value);
        }

        private int _suspiciousFiles;
        public int SuspiciousFiles
        {
            get => _suspiciousFiles;
            set => SetProperty(ref _suspiciousFiles, value);
        }
    }

}