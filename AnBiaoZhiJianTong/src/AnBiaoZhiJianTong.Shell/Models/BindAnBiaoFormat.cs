using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text.Json.Serialization;
using AnBiaoZhiJianTong.Models;
using Newtonsoft.Json;

namespace AnBiaoZhiJianTong.Shell.Models
{
    public class BindMatchAnBiaoFormatNode
    {
        public string CatalogName { get; set; } = string.Empty;

        public List<BindMatchSubCatalog> SubCatalogs { get; set; } = new List<BindMatchSubCatalog>();
    }

    public class BindMatchSubCatalog
    {
        public string CatalogName { get; set; } = string.Empty;

        public List<BindMatchRule> Rules { get; set; } = new List<BindMatchRule>();
    }

    public class BindMatchRule : MatchRule
    {
        // 用于绑定NodeValue的 ComboBox 的集合
        public ObservableCollection<NodeValueItem> ValueItems
        {
            get
            {
                if (string.IsNullOrWhiteSpace(NodeValue))
                    return new ObservableCollection<NodeValueItem>();

                try
                {
                    var nodeValueItems = JsonConvert.DeserializeObject<List<NodeValueItem>>(NodeValue);
                    return new ObservableCollection<NodeValueItem>(nodeValueItems ?? new List<NodeValueItem>());
                }
                catch
                {
                    return new ObservableCollection<NodeValueItem>();
                }
            }
        }

        // 用于绑定NodeUnit的 ComboBox 的集合
        public ObservableCollection<NodeValueItem> UnitItems
        {
            get
            {
                if (string.IsNullOrWhiteSpace(NodeUnit))
                    return new ObservableCollection<NodeValueItem>();

                try
                {
                    var nodeUnitItems = JsonConvert.DeserializeObject<List<NodeValueItem>>(NodeUnit);
                    return new ObservableCollection<NodeValueItem>(nodeUnitItems ?? new List<NodeValueItem>());
                }
                catch
                {
                    return new ObservableCollection<NodeValueItem>();
                }
            }
        }

        // 用于绑定NodeUnit的 TextBlock 展示时的可读字符串
        public string UnitText =>
            UnitItems?.Count > 0 
                ? string.Join(" / ", UnitItems.Select(i => i.CodeText))     //把每个项的 CodeText 取出来，用 " / " 拼成一行，比如 厘米 / 磅 / 行
                : string.Empty;

        // 用于绑定NodeValue的 ComboBox 的已选项
        public string SelectedValue { get; set; }

        // 用于绑定NodeUnit的 ComboBox 的已选项
        public string SelectedUnit { get; set; }

        // 用于绑定TextBox的默认值
        public string InputValue { get; set; }

        // 用于绑定CheckBox的默认值
        public bool IsYes { get; set; }
    }

    // 定义 NodeValue 的 JSON 对象的结构
    public class NodeValueItem
    {
        [JsonPropertyName("codeText")] 
        public string CodeText { get; set; }
        [JsonPropertyName("codeValue")] 
        public string CodeValue { get; set; }
    }

}
