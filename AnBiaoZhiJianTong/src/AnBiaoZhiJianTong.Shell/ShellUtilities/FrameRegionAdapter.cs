using System.Linq;
using System.Windows.Controls;
using Prism.Regions;

namespace AnBiaoZhiJianTong.Shell.ShellUtilities
{
    /// <summary>
    /// 让 Prism Region 可以把视图(Page/UserControl)呈现到 WPF Frame 里。
    /// - 如果是 Page：调用 frame.Navigate(page)，进入导航栈（可后退/前进）
    /// - 如果是非 Page（UserControl 等）：直接 frame.Content = view
    /// </summary>
    public sealed class FrameRegionAdapter : RegionAdapterBase<Frame>
    {
        public FrameRegionAdapter(IRegionBehaviorFactory regionBehaviorFactory)
            : base(regionBehaviorFactory) { }

        protected override void Adapt(IRegion region, Frame regionTarget)
        {
            // 当 Region 的激活视图变化时，把最新的 ActiveView 显示到 Frame 里
            region.ActiveViews.CollectionChanged += (s, e) =>
            {
                var active = region.ActiveViews.Cast<object>().LastOrDefault();
                if (active == null) return;

                if (active is Page page)
                {
                    // 避免重复导航
                    if (!ReferenceEquals(regionTarget.Content, page))
                        regionTarget.Navigate(page);
                }
                else
                {
                    // 不是 Page 的话直接塞 Content（不进导航栈）
                    if (!ReferenceEquals(regionTarget.Content, active))
                        regionTarget.Content = active;
                }
            };
        }

        protected override IRegion CreateRegion()
        {
            // 普通 Region 即可（需要导航历史可自行换为 JournalAwareRegion 的自定义实现）
            return new Region();
        }
    }
}
