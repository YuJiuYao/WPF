using System;
using System.IO;
using System.Threading;
using Aspose.Words;
using Aspose.Words.Loading;
using Aspose.Words.Saving;
using Aspose.Words.Settings;

namespace AnBiaoZhiJianTong.Common.Utilities
{
    /// <summary>
    ///  Word 转 PDF 工具类（Aspose.Words）
    /// </summary>
    public class Word2Pdf
    {
        private readonly string _docPath;
        private readonly string _pdfPath;
        private readonly Word2PdfOptions _options;

        /// <summary>
        /// 构造方法：检查路径是否是有效的 Word 文件
        /// </summary>
        /// <param name="docPath">Word 文件路径</param>
        /// <param name="pdfPath">输出 PDF 路径</param>
        /// <param name="options">转换配置</param>
        public Word2Pdf(string docPath, string pdfPath, Word2PdfOptions options = null)
        {
            if (string.IsNullOrWhiteSpace(docPath))
                throw new ArgumentNullException(nameof(docPath));
            if (!File.Exists(docPath))
                throw new FileNotFoundException("指定的 Word 文件不存在！", docPath);

            // 校验扩展名（支持 .doc/.docx）
            var ext = Path.GetExtension(docPath).ToLowerInvariant();
            if (ext != ".doc" && ext != ".docx")
                throw new ArgumentException("输入文件必须是 Word 文档（.doc 或 .docx）。", nameof(docPath));

            if (string.IsNullOrWhiteSpace(pdfPath))
                throw new ArgumentNullException(nameof(pdfPath));

            _docPath = docPath;
            _pdfPath = pdfPath;
            _options = options ?? Word2PdfOptions.FastBalanced();
        }


        /// <summary>
        /// 执行转换
        /// </summary>
        public void Convert(CancellationToken ct = default)
        {
            ct.ThrowIfCancellationRequested();

            var load = new LoadOptions
            {
                // 若需要，可在此定制编码、密码等
            };

            var doc = new Document(_docPath, load);

            // 文档级优化：对新版版面与运行合并等做兼容优化（可减渲染成本）
            doc.CompatibilityOptions.OptimizeFor(MsWordVersion.Word2016);

            // 适度清理（减少样式/列表等冗余，略降内存与体积）
            if (_options.Cleanup)
            {
                var cleanup = new CleanupOptions { UnusedStyles = true, UnusedLists = true, DuplicateStyle = true };
                doc.Cleanup(cleanup);
            }

            var pdf = new PdfSaveOptions
            {
                SaveFormat = SaveFormat.Pdf,
                Compliance = _options.UsePdfA ? PdfCompliance.PdfA1b : PdfCompliance.Pdf17,
                EmbedFullFonts = _options.EmbedFullFonts,                   // 建议 false：仅嵌入子集 → 更小更快
                OptimizeOutput = true,                                      // 关键：让 Aspose 做结构优化
                UseHighQualityRendering = _options.HighQualityRendering,    // 关闭可略提速
                ImageCompression = PdfImageCompression.Jpeg,
                JpegQuality = _options.JpegQuality,
                ExportDocumentStructure = _options.ExportDocumentStructure, // 无障碍/结构化，若不需要可设 false 提速减体积
                // 图片降采样（对体积/速度影响显著）
                DownsampleOptions =
                {
                    DownsampleImages = _options.DownsampleImages,
                    Resolution = _options.DownsampleResolutionDpi,
                    ResolutionThreshold = _options.DownsampleThresholdDpi
                }
            };

            // 将渲染过程的临时文件输出到高速磁盘，降低内存峰值
            if (!string.IsNullOrWhiteSpace(_options.TempFolder))
            {
                Directory.CreateDirectory(_options.TempFolder);
                pdf.TempFolder = _options.TempFolder;
            }

            // 原子写：先写临时文件，再替换
            var tmp = _pdfPath + ".tmp";
            doc.Save(tmp, pdf);
            ReplaceAtomically(tmp, _pdfPath);
        }

        private static void ReplaceAtomically(string tempFile, string finalFile)
        {
            // 若目标已存在，先删除/替换
            if (File.Exists(finalFile))
            {
                // 尝试原子替换；不同文件系统表现不同，保底用 Move 覆盖
                try { File.Replace(tempFile, finalFile, null); return; } catch { /* ignore */ }
                try { File.Delete(finalFile); } catch { /* ignore */ }
            }
            File.Move(tempFile, finalFile);
        }
    }

    /// <summary>
    /// 输出质量/速度/体积的关键参数
    /// </summary>
    public sealed class Word2PdfOptions
    {
        public bool UsePdfA { get; set; }                        // 不需要归档则 false：更快
        public bool EmbedFullFonts { get; set; }                 // false → 仅嵌入子集（更小）
        public bool ExportDocumentStructure { get; set; }
        public bool HighQualityRendering { get; set; } = true;   // 追求速度可设 false
        public bool Cleanup { get; set; } = true;

        public bool DownsampleImages { get; set; } = true;
        public int DownsampleResolutionDpi { get; set; } = 220;  // 打印友好且体积可控
        public int DownsampleThresholdDpi { get; set; } = 220;
        public int JpegQuality { get; set; } = 80;

        public string TempFolder { get; set; }                   // 指向 NVMe 或 RAM 磁盘更佳

        /// <summary>
        /// 预设FastBalanced：平衡质量+速度+体积
        /// </summary>
        /// <returns></returns>
        public static Word2PdfOptions FastBalanced() => new Word2PdfOptions
        {
            UsePdfA = false,
            EmbedFullFonts = false,
            ExportDocumentStructure = false,
            HighQualityRendering = true,
            Cleanup = true,
            DownsampleImages = true,
            DownsampleResolutionDpi = 220,
            DownsampleThresholdDpi = 220,
            JpegQuality = 80,
            TempFolder = Path.Combine(Path.GetTempPath(), "AsposeWTemp")
        };

        /// <summary>
        /// 预设SmallerAndFaster：更快/更小（网络分发）
        /// </summary>
        /// <returns></returns>
        public static Word2PdfOptions SmallerAndFaster() => new Word2PdfOptions
        {
            UsePdfA = false,
            EmbedFullFonts = false,
            ExportDocumentStructure = false,
            HighQualityRendering = false,
            Cleanup = true,
            DownsampleImages = true,
            DownsampleResolutionDpi = 180,
            DownsampleThresholdDpi = 180,
            JpegQuality = 70,
            TempFolder = Path.Combine(Path.GetTempPath(), "AsposeWTemp")
        };

        /// <summary>
        /// 预设PrintFriendly：打印
        /// </summary>
        /// <returns></returns>
        public static Word2PdfOptions PrintFriendly() => new Word2PdfOptions
        {
            UsePdfA = true,
            EmbedFullFonts = true,
            ExportDocumentStructure = true,
            HighQualityRendering = true,
            Cleanup = true,
            DownsampleImages = true,
            DownsampleResolutionDpi = 240,
            DownsampleThresholdDpi = 240,
            JpegQuality = 85,
            TempFolder = Path.Combine(Path.GetTempPath(), "AsposeWTemp")
        };
    }
}
