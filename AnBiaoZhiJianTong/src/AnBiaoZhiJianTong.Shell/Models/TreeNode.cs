using System.Collections.ObjectModel;
using Prism.Mvvm;

namespace AnBiaoZhiJianTong.Shell.Models
{
    public class TreeNode : BindableBase
    {
        private string _number;
        public string Number
        {
            get => _number;
            set => SetProperty(ref _number, value);
        }

        private string _title;
        public string Title
        {
            get => _title;
            set => SetProperty(ref _title, value);
        }

        // 三态勾选：true/false/null(null=部分选中)
        private bool? _isChecked = false;
        public bool? IsChecked
        {
            get => _isChecked;
            set
            {
                if (SetProperty(ref _isChecked, value))
                {
                    // 子级联动
                    if (value.HasValue) SetIsCheckedForChildren(value.Value);
                    // 回溯父级，计算三态
                    Parent?.UpdateCheckStateFromChildren();
                }
            }
        }

        private bool _isExpanded;
        public bool IsExpanded
        {
            get => _isExpanded;
            set => SetProperty(ref _isExpanded, value);
        }

        private bool _isSelected;
        public bool IsSelected
        {
            get => _isSelected;
            set => SetProperty(ref _isSelected, value);
        }

        private ObservableCollection<TreeNode> Children { get; } = new ObservableCollection<TreeNode>();

        public TreeNode Parent { get; private set; }


        private void SetIsCheckedForChildren(bool value)
        {
            foreach (var c in Children)
            {
                c._isChecked = value;     // 直接设字段避免重复回溯
                c.RaisePropertyChanged(nameof(IsChecked));
                c.SetIsCheckedForChildren(value);
            }
        }

        private void UpdateCheckStateFromChildren()
        {
            if (Children.Count == 0) return;

            int trueCount = 0, falseCount = 0, nullCount = 0;
            foreach (var c in Children)
            {
                if (c.IsChecked == true) trueCount++;
                else if (c.IsChecked == false) falseCount++;
                else nullCount++;
            }

            bool? newState;
            if (trueCount == Children.Count) newState = true;
            else if (falseCount == Children.Count) newState = false;
            else newState = null;

            if (_isChecked != newState)
            {
                _isChecked = newState;
                RaisePropertyChanged(nameof(IsChecked));
                Parent?.UpdateCheckStateFromChildren();
            }
        }
        public void AddChild(TreeNode child)
        {
            if (child == null) return;
            child.Parent = this;
            Children.Add(child);
        }

    }
}