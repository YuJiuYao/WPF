using System;
using System.IO;
using System.Net.NetworkInformation;
using System.Reflection;
using System.Security.Cryptography;
using System.Text;
using System.Text.Encodings.Web;
using System.Text.Json;
using System.Text.RegularExpressions;
using System.Text.Unicode;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Win32;

namespace AnBiaoZhiJianTong.Common.Utilities
{
    /// <summary>
    /// 通用功能方法类
    /// </summary>
    public static class FunctionHelper
    {
        /// <summary>
        /// 转换Json文件为对应Model
        /// </summary>
        /// <typeparam name="T">Json的Model</typeparam>
        /// <param name="jsonPath">需要转换的Json文件路径</param>
        /// <returns></returns>
        /// <exception cref="ArgumentException">JSON 文件路径为空</exception>
        /// <exception cref="FileNotFoundException">未找到指定的 JSON 文件</exception>
        public static async Task<T> Json2ModelAsync<T>(string jsonPath)
        {
            if (string.IsNullOrWhiteSpace(jsonPath))
                throw new ArgumentException("JSON 文件路径不能为空", nameof(jsonPath));

            if (!File.Exists(jsonPath))
                throw new FileNotFoundException("未找到指定的 JSON 文件", jsonPath);

            using (var fs = File.OpenRead(jsonPath))
            {
                var options = new JsonSerializerOptions
                {
                    PropertyNameCaseInsensitive = true,                     // 忽略属性大小写
                    Encoder = JavaScriptEncoder.Create(UnicodeRanges.All),  // 支持中文等所有 Unicode 字符
                    ReadCommentHandling = JsonCommentHandling.Skip,         // 忽略 JSON 文件中的注释
                    AllowTrailingCommas = true                              // 允许尾随逗号
                };

                return await JsonSerializer.DeserializeAsync<T>(fs, options);
            }
        }

        /// <summary>
        /// 从指定对象上获取指定候选属性的值（泛型返回）
        /// </summary>
        /// <typeparam name="T">属性值的返回类型</typeparam>
        /// <param name="obj">指定对象</param>
        /// <param name="name">指定属性</param>
        /// <param name="defaultValue">当属性不存在或取值失败时的默认值</param>
        /// <returns>返回指定属性的值，如果获取失败则返回 <paramref name="defaultValue"/></returns>
        /// <exception cref="ArgumentNullException">当 <paramref name="obj"/> 为空时抛出</exception>
        /// <exception cref="ArgumentException">当 <paramref name="name"/> 为空或仅空白字符时抛出</exception>
        public static T GetPropValue<T>(object obj, string name, T defaultValue = default)
        {
            if (obj == null) throw new ArgumentNullException(nameof(obj));
            if (string.IsNullOrWhiteSpace(name)) throw new ArgumentException("属性名不能为空", nameof(name));

            // 获取对象类型
            var type = obj.GetType();

            // 查找指定名称的属性（忽略大小写）
            var prop = type.GetProperty(name, BindingFlags.Public | BindingFlags.Instance | BindingFlags.IgnoreCase);
            if (prop == null)
            {
                return defaultValue;
            }

            try
            {
                // 获取属性值
                var value = prop.GetValue(obj);

                // 如果值为 null
                if (value == null)
                    return defaultValue;

                // 如果类型已经匹配，则直接返回
                if (value is T tValue)
                    return tValue;

                // 尝试类型转换
                return (T)Convert.ChangeType(value, typeof(T));
            }
            catch
            {
                // 任意异常返回默认值
                return defaultValue;
            }
        }


        /// <summary>
        /// 解析 jsonStr，若是被转义的 JSON 先反解再获取 jsonName 字段值，否则直接返回
        /// </summary>
        /// <param name="jsonStr">JSON 字符串或纯文本</param>
        /// <param name="jsonName">要获取的字段名</param>
        /// <returns>字段值或原文本</returns>
        public static string ExtractDisplayValue(string jsonStr, string jsonName)
        {
            if (string.IsNullOrWhiteSpace(jsonStr)) return string.Empty;

            var jsonTrimmed = jsonStr.Trim();
            // 如果不是 JSON 格式，直接返回    
            if (!IsJson(jsonTrimmed)) return jsonStr;
            
            try
            {
                using (var jsonDocument = JsonDocument.Parse(jsonStr))
                {
                    if (jsonDocument.RootElement.TryGetProperty(jsonName, out var prop))
                        return prop.GetString() ?? string.Empty;
                }
            }
            catch
            {
                return jsonStr;
            }

            return jsonStr;
        }


        /// <summary>
        /// 判断输入字符串是否为有效的 JSON 格式（对象或数组）
        /// </summary>
        /// <param name="inputStr">待检测的字符串</param>
        /// <returns></returns>
        private static bool IsJson(string inputStr)
        {
            if (string.IsNullOrWhiteSpace(inputStr)) return false;

            try
            {
                using (JsonDocument.Parse(inputStr)) { }
                return true;
            }
            catch
            {
                return false;
            }
        }



        /// <summary>
        /// 显示“另存为”对话框，并将 <paramref name="sourcePath"/> 复制到用户选择的位置。
        /// </summary>
        /// <param name="sourcePath">源文件的完整路径（必须存在）。</param>
        /// <param name="defaultFileName">对话框默认文件名；为空时取 <paramref name="sourcePath"/> 的文件名。</param>
        /// <param name="renameStripGuid">如果为 true，则把文件名中扩展名前的“_GUID”后缀去掉。</param>
        /// <param name="subdirectoryName">如果非空，则在用户选择的目录下创建该子目录，并把目标文件保存到该子目录内。</param>
        /// <param name="title">对话框标题（默认：导出文档）。</param>
        /// <param name="overwriteIfExists">是否覆盖已存在文件。</param>
        /// <param name="filter">文件过滤器（默认：Word 文档 与 所有文件）。</param>
        /// <param name="initialDirectory">初始目录；为空则使用“我的文档”。</param>
        /// <param name="isOpenSaveDirectory">是否打开保存目录；。</param>
        /// <param name="isOpenSaveFile">是否打开保存文件。</param>
        /// <param name="token">取消令牌（可选）。</param>
        /// <returns>返回一次保存操作的结果。</returns>
        public static async Task<FileSaveResult> ShowSaveAsAndCopyAsync(
            string sourcePath,
            string defaultFileName,
            bool renameStripGuid = false,
            string subdirectoryName = null,
            bool overwriteIfExists = true,
            string title = "导出文档",
            string filter = "Word 文档 (*.docx;*.doc)|*.docx;*.doc|所有文件|*.*",
            string initialDirectory = null,
            bool isOpenSaveDirectory = false,
            bool isOpenSaveFile = false,
            CancellationToken token = default)
        {

            // 0) 基本校验：源文件必须存在
            if (string.IsNullOrWhiteSpace(sourcePath) || !File.Exists(sourcePath))
            {
                return new FileSaveResult { Success = false, ErrorMessage = "源文件不存在。" };
            }

            // 1) 计算对话框默认文件名（若启用重命名，则对默认名先做一次“去 GUID 后缀”）
            var srcName = string.IsNullOrWhiteSpace(defaultFileName)
                ? Path.GetFileName(sourcePath)
                : defaultFileName;
            if (renameStripGuid && !string.IsNullOrWhiteSpace(srcName))
            {
                srcName = StripGuidSuffixFromFileName(srcName);
            }

            // 2) UI 线程弹出另存为对话框（SaveFileDialog 内部会阻塞当前线程，正常用法）
            var sfd = new SaveFileDialog
            {
                Title = title,
                FileName = srcName,
                Filter = filter,
                AddExtension = true,
                OverwritePrompt = true,
                InitialDirectory = string.IsNullOrWhiteSpace(initialDirectory)
                                     ? Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments)
                                     : initialDirectory
            };

            var ok = sfd.ShowDialog() == true;
            if (!ok)
                return new FileSaveResult { Success = false, Canceled = true };

            // 3) 判断是否创建目录
            var chosenDir = Path.GetDirectoryName(sfd.FileName);
            if (string.IsNullOrEmpty(chosenDir))
                return new FileSaveResult { Success = false, ErrorMessage = "未能获取选择的目录。" };

            var finalDir = chosenDir;
            if (!string.IsNullOrWhiteSpace(subdirectoryName))
            {
                finalDir = Path.Combine(chosenDir, subdirectoryName);
            }

            var finalFileName = Path.GetFileName(sfd.FileName); // 用户在对话框可能修改了名称与扩展
            if (renameStripGuid)
            {
                finalFileName = StripGuidSuffixFromFileName(finalFileName);
            }

            // 确保目录存在
            Directory.CreateDirectory(finalDir);

            var targetPath = Path.Combine(finalDir, finalFileName);

            if (!overwriteIfExists && File.Exists(targetPath))
            {
                targetPath = EnsureUniquePath(finalDir, finalFileName); // name(1).ext 递增直到可用
            }

            try
            {
                // 4) 后台线程执行复制（避免阻塞 UI；支持取消）
                await Task.Run(() =>
                {
                    token.ThrowIfCancellationRequested();
                    File.Copy(sourcePath, targetPath, overwriteIfExists);
                }, token);
                if (isOpenSaveDirectory)
                {
                    // 打开资源管理器并选中文件（失败也不影响主流程）
                    try { System.Diagnostics.Process.Start("explorer.exe", "/select,\"" + targetPath + "\""); }
                    catch { /*// ignored*/ }
                }
                if (isOpenSaveFile)
                {
                    try
                    {
                        var psi = new System.Diagnostics.ProcessStartInfo
                        {
                            FileName = targetPath,
                            UseShellExecute = true, // 交由系统根据扩展名选择默认程序
                            Verb = "open"           // 显式指定“打开”
                        };
                        System.Diagnostics.Process.Start(psi);
                    }
                    catch
                    {
                        // 打开失败时降级为仅打开目录（不抛异常干扰主流程）
                        try
                        {
                            System.Diagnostics.Process.Start("explorer.exe", "/select,\"" + targetPath + "\"");
                        }
                        catch { /* ignored */ }
                    }
                }

                return new FileSaveResult { Success = true, TargetPath = targetPath };
            }
            catch (OperationCanceledException)
            {
                // 调用方或用户触发了取消
                return new FileSaveResult { Success = false, Canceled = true };
            }
            catch (Exception ex)
            {
                // 其他异常（如目标被占用、无权限等）
                return new FileSaveResult { Success = false, ErrorMessage = ex.Message };
            }
        }


        /// <summary>
        /// 去掉文件名（不含目录）的扩展名前 “_GUID” 后缀。
        /// 例如： "工程_977d7c6c-84db-4ccc-995a-134cd4dae029.docx" → "工程.docx"
        /// 对不匹配的文件名保持不变。
        /// </summary>
        private static string StripGuidSuffixFromFileName(string fileName)
        {
            if (string.IsNullOrWhiteSpace(fileName)) return fileName;

            var name = Path.GetFileNameWithoutExtension(fileName);
            var ext = Path.GetExtension(fileName);

            var lastUnderscore = name.LastIndexOf('_');
            if (lastUnderscore >= 0)
            {
                name = name.Substring(0, lastUnderscore);
            }

            return string.Concat(name, ext);
        }

        /// <summary>
        /// 若 directory 下存在同名 fileName，则生成不冲突的新路径：
        /// name.ext → name(1).ext → name(2).ext → ...
        /// </summary>
        private static string EnsureUniquePath(string directory, string fileName)
        {
            var baseName = Path.GetFileNameWithoutExtension(fileName);
            var ext = Path.GetExtension(fileName);

            string candidate = Path.Combine(directory, fileName);
            int i = 1;
            while (File.Exists(candidate))
            {
                candidate = Path.Combine(directory, $"{baseName}({i}){ext}");
                i++;
            }
            return candidate;
        }

        /// <summary>
        /// 复制sourcePath的文件到指定目录targetDir下，若目录不存在则创建。
        /// </summary>
        /// <param name="sourcePath"></param>
        /// <param name="defaultFileName"></param>
        /// <param name="targetDir"></param>
        /// <param name="token"></param>
        /// <returns></returns>
        public static async Task<FileSaveResult> CopyFileToTargetDirAsync(
            string sourcePath,
            string targetDir,
            string defaultFileName = null,
            CancellationToken token = default)
        {
            var result = new FileSaveResult
            {
                Success = false,
                TargetDirectory = targetDir
            };

            if (string.IsNullOrWhiteSpace(sourcePath) || !File.Exists(sourcePath))
            {
                result.ErrorMessage = "源文件不存在";
                return result;
            }

            try
            {
                // 确保目标目录存在
                if (!Directory.Exists(targetDir))
                    Directory.CreateDirectory(targetDir);

                // 确定目标文件名
                var fileName = string.IsNullOrWhiteSpace(defaultFileName)
                    ? Path.GetFileName(sourcePath)
                    : defaultFileName;

                var destPath = Path.Combine(targetDir, fileName);

                // 异步复制
                using (var sourceStream = new FileStream(sourcePath, FileMode.Open, FileAccess.Read, FileShare.Read, 81920, useAsync: true))
                using (var destStream = new FileStream(destPath, FileMode.Create, FileAccess.Write, FileShare.None, 81920, useAsync: true))
                {
                    await sourceStream.CopyToAsync(destStream, 81920, token);
                }

                result.Success = true;
                result.TargetPath = destPath;
                return result;
            }
            catch (OperationCanceledException)
            {
                result.Canceled = true;
                result.ErrorMessage = "操作被取消";
                return result;
            }
            catch (Exception ex)
            {
                result.ErrorMessage = $"复制失败: {ex.Message}";
                return result;
            }
        }


        public static string Rsa2048Sign(string data)
        {
            // 使用之前生成的私钥参数进行签名
            byte[] dataBytes = Encoding.UTF8.GetBytes(data);
            byte[] signatureBytes = RsaKeyHelper.LoadPrivateKey(Path.Combine(AppContext.BaseDirectory, "configs", "private.pem")).SignData(dataBytes, HashAlgorithmName.SHA256, RSASignaturePadding.Pkcs1);
            return Convert.ToBase64String(signatureBytes);
        }

        public static string GenerateDeviceFingerprint()
        {
            string cpu = GetCpuInfo();
            string memory = GetMemoryInfo();
            string disk = GetDiskInfo();
            string mac = GetMacAddress();
            string osInfo = GetOperatingSystemInfo();

            // 构建设备指纹字符串
            StringBuilder deviceInfo = new StringBuilder();
            deviceInfo.Append("MAC:").Append(mac).Append("|");
            deviceInfo.Append("OS:").Append(osInfo).Append("|");
            deviceInfo.Append("CPU:").Append(cpu).Append("|");
            deviceInfo.Append("MEM:").Append(memory).Append("|");
            deviceInfo.Append("DISK:").Append(disk).Append("|");
            string Info = deviceInfo.ToString();

            // 计算SHA-256哈希
            using (SHA256 sha256 = SHA256.Create())
            {
                byte[] hashBytes = sha256.ComputeHash(Encoding.UTF8.GetBytes(Info));
                StringBuilder sb = new StringBuilder();
                foreach (byte b in hashBytes)
                {
                    sb.Append(b.ToString("x2"));
                }
                return sb.ToString();
            }
        }

        private static string GetCpuInfo()
        {
            try
            {
                string cpuInfo = string.Empty;
                using (var searcher = new System.Management.ManagementObjectSearcher("SELECT * FROM Win32_Processor"))
                {
                    foreach (var item in searcher.Get())
                    {
                        cpuInfo = item["Name"].ToString();
                        break; // 只取第一个CPU信息
                    }
                }
                return string.IsNullOrWhiteSpace(cpuInfo) ? "unknown" : cpuInfo;
            }
            catch (Exception)
            {
                return "unknown";
            }
        }

        private static string GetMemoryInfo()
        {
            try
            {
                ulong totalPhysicalMemory = new Microsoft.VisualBasic.Devices.ComputerInfo().TotalPhysicalMemory;
                return totalPhysicalMemory.ToString();
            }
            catch (Exception)
            {
                return "unknown";
            }
        }

        private static string GetDiskInfo()
        {
            try
            {
                StringBuilder diskInfo = new StringBuilder();
                foreach (System.IO.DriveInfo drive in System.IO.DriveInfo.GetDrives())
                {
                    if (drive.IsReady)
                    {
                        diskInfo.Append(drive.Name).Append(":").Append(drive.TotalSize).Append(";");
                    }
                }
                return diskInfo.ToString();
            }
            catch (Exception)
            {
                return "unknown";
            }
        }

        private static string GetMacAddress()
        {
            try
            {
                string macAddress = string.Empty;
                foreach (NetworkInterface nic in NetworkInterface.GetAllNetworkInterfaces())
                {
                    // 跳过虚拟网卡和回环接口
                    if (nic.NetworkInterfaceType == NetworkInterfaceType.Loopback)
                    {
                        continue;
                    }
                    if (nic.OperationalStatus == OperationalStatus.Up)
                    {
                        macAddress = nic.GetPhysicalAddress().ToString();
                        break;
                    }
                }
                return string.IsNullOrWhiteSpace(macAddress) ? "unknown" : macAddress;
            }
            catch (Exception)
            {
                return "unknown";
            }
        }
        private static string GetOperatingSystemInfo()
        {
            try
            {
                string osName = Environment.OSVersion.Platform.ToString();
                string osVersion = Environment.OSVersion.Version.ToString();
                string osArch = Environment.Is64BitOperatingSystem ? "x64" : "x86";

                return $"{osName}_{osVersion}_{osArch}";
            }
            catch (Exception)
            {
                return "unknown";
            }
        }
    }


    public sealed class FileSaveResult
    {
        public bool Success { get; set; }
        public bool Canceled { get; set; }
        public string TargetPath { get; set; }
        public string ErrorMessage { get; set; }
        public string TargetDirectory { get; set; }
    }
}
