using System.Collections.Generic;
using System.Text.Json.Serialization;

namespace AnBiaoZhiJianTong.Shell.Models
{
    /// <summary>
    /// anbiaoformat Json模型
    /// </summary>
    public class AnBiaoFormat
    {
        [JsonPropertyName("anbiaoformat")]
        public AnBiaoRoot AnBiaoRoot { get; set; }
    }

    /// <summary>
    /// 根节点
    /// </summary>
    public class AnBiaoRoot
    {
        [JsonPropertyName("sites")]
        public List<Site> Sites { get; set; } = new List<Site>();
    }

    /// <summary>
    /// site 节点
    /// </summary>
    public class Site
    {
        /// <summary>
        /// 规则分类名称（例如：封面要求、目录要求等）
        /// </summary>
        [JsonPropertyName("sitename")]
        public string SiteName { get; set; } = string.Empty;

        /// <summary>
        /// 当前分类下的所有规则节点
        /// </summary>
        [JsonPropertyName("nodes")]
        public List<Node> Nodes { get; set; } = new List<Node>();
    }

    /// <summary>
    /// node 节点
    /// </summary>
    public class Node
    {
        /// <summary>
        /// 检测项名称（例如：封面要求、目录要求等）
        /// </summary>
        [JsonPropertyName("nodename")]
        public string NodeName { get; set; } = string.Empty;

        /// <summary>
        /// 检测内容（例如“中文字体名称，字号等”）
        /// </summary>
        [JsonPropertyName("nodevalue")]
        public string NodeValue { get; set; } = string.Empty;

        /// <summary>
        /// 
        /// </summary>
        [JsonPropertyName("isrengong")]
        public string IsRenGong { get; set; } = string.Empty; // "是" / "否"

        /// <summary>
        /// 
        /// </summary>
        [JsonPropertyName("nodecode")]
        public string NodeCode { get; set; } = string.Empty;
    }

}
