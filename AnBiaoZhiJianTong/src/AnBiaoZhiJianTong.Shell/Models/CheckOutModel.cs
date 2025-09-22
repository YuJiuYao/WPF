using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Drawing;
using AnBiaoZhiJianTong.Shell.Views;

namespace AnBiaoZhiJianTong.Shell.Models
{
    public class ErrorInfo
    {
        public int Index { get; set; }
        public string Keyword { get; set; }
        public string Description { get; set; }
        public int PageNumber { get; set; }
        public string Status { get; set; }
    }

    public class MyTreeNode
    {
        public int TotalCount { get; set; }  // 总问题数量（ErrorCount + ManualCount）
        public int ErrorCount { get; set; }  // 错误项数量
        public int ManualCount { get; set; } // 人工检查项数量
        public string Number { get; set; }
        public string Title { get; set; }
        public bool IsChecked { get; set; }
        public ObservableCollection<MyTreeNode> Children { get; set; }
        public ObservableCollection<MyTreeNodeData> AssociatedDataItems { get; set; } = new ObservableCollection<MyTreeNodeData>();

        private object _middleContent;
        public object MiddleContent
        {
            get
            {
                if (_middleContent == null)
                {
                    // 只有首次访问时创建实际控件
                    _middleContent = BuildMiddleContent();
                }
                return _middleContent;
            }
        }
        // 实际UI控件（延迟加载）
        private object _rightContent;
        public object RightContent
        {
            get
            {
                if (_rightContent == null)
                {
                    // 只有首次访问时创建实际控件
                    _rightContent = BuildRightContent();
                }
                return _rightContent;
            }
        }
        public MyTreeNode(string number, string title)
        {
            Number = number;
            Title = title;
            Children = new ObservableCollection<MyTreeNode>();
        }
        private object BuildMiddleContent()
        {
            var page = new ErrorCheckPage();
            return page;
        }
        private object BuildRightContent()
        {
            var page = new ErrorShowPage();
            return page;
        }
    }
    public class MyTreeNodeData
    {
        public int Id { get; set; }
        public string DocumentName { get; set; }        // 文件名称
        public string ErrorPageNumber { get; set; }        // 错误页码
        public string CheckPointName { get; set; }      // 检查点名称
        public string CheckPointRequirement { get; set; } // 检查要求
        public string ErrorSummary { get; set; }        // 错误点摘要
        public string ErrorDetail { get; set; }         // 错误详细信息
        public string ErrorParagraph { get; set; }      // 错误附近的段落
        public string ErrorRun { get; set; }            // 错误附近的内容
        public string SectionIndex { get; set; }           // 错误所在的章节
        public string CheckPointTypeCode {  get; set; } //  检查项类型

        public string CheckResult {  get; set; } //检查结果

        public string NodeCode {  get; set; } //检查点大类

        public Brush TyprColor { get; set; }//显示颜色

        public List<MyTreeNodeData> Children { get; set; }
    }
}
