using System;
using System.ComponentModel;
using System.Windows.Media;
using System.Windows.Media.Imaging;

namespace AnBiaoZhiJianTong.Common.Utilities
{
    public static class LoadIco
    {
        //icos文件所在的路径
        private const string BaseUri = "pack://application:,,,/Properties/Icons/";
        public static ImageSource LoadIcon(string iconName)
        {
            try
            {
                var uri = new Uri($"{BaseUri}{iconName}.ico", UriKind.Absolute);
                var bitmap = new BitmapImage(uri);
                bitmap.Freeze(); // 提升性能并避免跨线程问题
                return bitmap;
            }
            catch (Exception ex)
            {
                // 处理加载失败（可选：回退到默认图标）
                Console.WriteLine($"图标加载失败: {ex.Message}");
                return LoadDefaultIcon();
            }
        }
        private static ImageSource LoadDefaultIcon()
        {
            // 返回一个内置默认图标或null
            return null;
        }
    }
}
