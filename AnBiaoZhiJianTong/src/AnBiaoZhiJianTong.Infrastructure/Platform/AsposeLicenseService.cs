using System.Reflection;
using AnBiaoZhiJianTong.Core.Contracts.Platform;


namespace AnBiaoZhiJianTong.Infrastructure.Platform
{
    public sealed class AsposeLicenseService : IAsposeLicenseService
    {
        public void EnsureLicensed()
        {
            var license = new Aspose.Words.License();
            var asm = Assembly.Load("AnBiaoZhiJianTong.Shell");
            using var stream = asm.GetManifestResourceStream(
                "AnBiaoZhiJianTong.Shell.Aspose.Words.NET.lic");
            license.SetLicense(stream);
        }
    }
}
