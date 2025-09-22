using System.Collections.Generic;
using System.Windows.Controls;
using AnBiaoZhiJianTong.Models;
using AnBiaoZhiJianTong.Models.LoginDTO;
using Prism.Events;

namespace AnBiaoZhiJianTong.Core.Events
{
    /// <summary>
    /// 规则匹配结果更新事件,用于在视图模型之间传递匹配后的 <see cref="List{MatchAnBiaoFormatNode}"/> 集合
    /// 继承自 <see cref="PubSubEvent{T}"/>，支持基于事件聚合器的发布/订阅通信。  
    /// 发布者：触发事件；  订阅者：订阅该事件以接收更新后的匹配结果数据。  
    /// </summary>
    public class MatchAnBiaoFormatNodesEvents : PubSubEvent<List<MatchAnBiaoFormatNode>> { }


    public class LoginInfoEvent : PubSubEvent<LogininApiResponse>
    {
    }

    public class PayUrlInfoEvent : PubSubEvent<List<string>>
    {
    }
    public class CloseWindowEvent : PubSubEvent { }

    /// <summary>
    /// 页面跳转
    /// </summary>
    public class NavigateToPageEvent : PubSubEvent<Page> { }


    /// <summary>
    /// 页面跳转并携带数据
    /// </summary>
    public sealed class NavigateToPageEventWithData : PubSubEvent<NavigateToPageRequest> { }

    /// <summary>
    /// 导航请求载荷：目标页 + 参数
    /// </summary>
    public sealed class NavigateToPageRequest
    {
        /// <summary>要显示的目标 Page 实例</summary>
        public Page TargetPage { get; set; }

        /// <summary>导航参数包</summary>
        public MyNavigationParameters Parameters { get; set; } = new MyNavigationParameters();
    }


    /// <summary>
    /// 轻量参数包：字典 + 强类型读取
    /// </summary>
    public sealed class MyNavigationParameters : Dictionary<string, object>
    {
        public T Get<T>(string key, T defaultValue = default)
            => TryGetValue(key, out var v) && v is T t ? t : defaultValue;

        public T Require<T>(string key) =>
            TryGetValue(key, out var v) && v is T t
                ? t
                : throw new KeyNotFoundException($"缺少参数或类型不匹配: {key} -> {typeof(T).Name}");
    }


    public interface IAcceptParameters
    {
        void ApplyParameters(MyNavigationParameters myNavigationParameters);
    }
}
