using System.Diagnostics;
using System.Reflection;
using System.Runtime.CompilerServices;
using System.Text.RegularExpressions;

namespace AnBiaoZhiJianTong.Common.Utilities
{

    /// <summary>
    /// 程序集版本获取工具（优先使用 AssemblyInformationalVersion）。
    /// 设计目标：从调用方角度返回“调用者自己的版本”，用于跨程序集通用调用。
    /// </summary>
    public static class AssemblyHelper
    {
        /// <summary>
        /// 获取目标程序集的版本号（优先 AssemblyInformationalVersion；备用 FileVersion / AssemblyName.Version）。
        /// </summary>
        /// <param name="semverOnly">
        /// true：仅返回 SemVer 主体（如 "1.5.0"），去除 "+sha"/后缀；false：返回完整 InformationalVersion（如 "1.5.0+f79aaf6a3c"）。
        /// </param>
        /// <param name="assembly">
        /// 可选：显式指定目标程序集；若为 null，则优先 EntryAssembly，否则使用 CallingAssembly。
        /// </param>
        public static string GetApplicationVersion(bool semverOnly = true, Assembly assembly = null)
        {
            var asm = assembly
                      ?? Assembly.GetEntryAssembly()
                      ?? GetCallingAssemblySafe();

            if (asm == null)
                return "1.0.0"; // 极端兜底

            // 1) InformationalVersion（NBGV 推荐读取此值）
            var info = asm.GetCustomAttribute<AssemblyInformationalVersionAttribute>()?.InformationalVersion;
            if (!string.IsNullOrWhiteSpace(info))
                return semverOnly ? ExtractSemVer(info) ?? info : info;

            // 2) FileVersion（文件属性版本）
            try
            {
                var loc = asm.Location; // 动态程序集可能抛异常/为空
                if (!string.IsNullOrEmpty(loc))
                {
                    var fvi = FileVersionInfo.GetVersionInfo(loc);
                    if (!string.IsNullOrWhiteSpace(fvi.FileVersion))
                        return semverOnly ? ExtractSemVer(fvi.FileVersion) ?? fvi.FileVersion : fvi.FileVersion;
                }
            }
            catch { /* ignore */ }

            // 3) AssemblyName.Version（四段 1.0.0.0）
            var ver = asm.GetName()?.Version?.ToString();
            if (!string.IsNullOrWhiteSpace(ver))
                return semverOnly ? ExtractSemVer(ver) ?? ver : ver;

            return "1.0.0";
        }

        /// <summary>
        /// 从字符串中提取 SemVer 主体（支持 3~4 段数字；忽略 -pre/+meta）。
        /// </summary>
        private static string ExtractSemVer(string versionText)
        {
            if (string.IsNullOrWhiteSpace(versionText)) return null;
            // 先匹配 3 段（1.2.3），若失败再尝试 4 段（1.2.3.4）
            var m = Regex.Match(versionText, @"^\d+\.\d+\.\d+");
            if (m.Success) return m.Value;

            m = Regex.Match(versionText, @"^\d+\.\d+\.\d+\.\d+");
            return m.Success ? m.Value : null;
        }

        /// <summary>
        /// 为了避免 JIT 内联影响，单独封装一层获取 CallingAssembly。
        /// </summary>
        [MethodImpl(MethodImplOptions.NoInlining)]
        private static Assembly GetCallingAssemblySafe() => Assembly.GetCallingAssembly();
    }
}
