using System.Collections.Generic;
using System.Text.Json.Serialization;

namespace AnBiaoZhiJianTong.Models
{
    /// <summary>
    /// 二级节点
    /// </summary>
    public class BaseAnBiaoFormatNode
    {
        [JsonPropertyName("目录名")] 
        public string CatalogName { get; set; } = string.Empty;

        [JsonPropertyName("二级目录")] 
        public List<SubCatalog> SubCatalogs { get; set; } = new List<SubCatalog>();
    }

    /// <summary>
    /// 二级目录节点
    /// </summary>
    public class SubCatalog
    {
        [JsonPropertyName("目录名")] 
        public string CatalogName { get; set; } = string.Empty;

        [JsonPropertyName("检测点")] 
        public List<Rule> Rules { get; set; } = new List<Rule>();
    }

    /// <summary>
    /// 检测点节点
    /// </summary>
    public class Rule
    {
        [JsonPropertyName("NodeCode")] 
        public string NodeCode { get; set; } = string.Empty;

        [JsonPropertyName("NodeName")] 
        public string NodeName { get; set; } = string.Empty;

        [JsonPropertyName("NodeValue")] 
        public string NodeValue { get; set; } = string.Empty;

        [JsonPropertyName("NodeUnit")] 
        public string NodeUnit { get; set; }

        [JsonPropertyName("Type")] 
        public string Type { get; set; } = string.Empty;

        [JsonPropertyName("CheckRequirementItem")]
        public string CheckRequirementItem { get; set; } = string.Empty;

        [JsonPropertyName("IsRenGong")] 
        public string IsRenGong { get; set; } = string.Empty;

        [JsonPropertyName("IsParent")] 
        public string IsParent { get; set; } = string.Empty;

        [JsonPropertyName("AttriBute")] 
        public string AttriBute { get; set; } = string.Empty;

        [JsonPropertyName("NodeInfo")] 
        public string NodeInfo { get; set; } = string.Empty;
    }



    public class MatchAnBiaoFormatNode
    {
        [JsonPropertyName("目录名")]
        public string CatalogName { get; set; } = string.Empty;

        [JsonPropertyName("二级目录")]
        public List<MatchSubCatalog> SubCatalogs { get; set; } = new List<MatchSubCatalog>();
    }

    /// <summary>
    /// 二级目录节点
    /// </summary>
    public class MatchSubCatalog
    {
        [JsonPropertyName("目录名")]
        public string CatalogName { get; set; } = string.Empty;

        [JsonPropertyName("检测点")]
        public List<MatchRule> Rules { get; set; } = new List<MatchRule>();

    }

    public class MatchRule : Rule
    {
        [JsonPropertyName("MatchValue")]
        public string MatchValue { get; set; } = string.Empty;
    }

}
