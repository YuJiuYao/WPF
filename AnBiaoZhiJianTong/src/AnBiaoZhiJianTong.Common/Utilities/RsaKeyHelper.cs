using System;
using System.IO;
using System.Security.Cryptography;
using System.Text;
using Org.BouncyCastle.Crypto.Parameters;
using Org.BouncyCastle.OpenSsl;

namespace AnBiaoZhiJianTong.Common.Utilities
{
    public static class RsaKeyHelper
    {
        private const string PrivateKeyHeader = "-----BEGIN RSA PRIVATE KEY-----";
        private const string PrivateKeyFooter = "-----END RSA PRIVATE KEY-----";
        private const string PublicKeyHeader = "-----BEGIN PUBLIC KEY-----";
        private const string PublicKeyFooter = "-----END PUBLIC KEY-----";

        public static void GenerateAndSaveKeyPair(string privateKeyPath, string publicKeyPath, int keySize = 2048)
        {
            using (var rsa = RSA.Create())
            {
                // 设置密钥大小
                rsa.KeySize = keySize;
                // 导出私钥和公钥
                SavePrivateKey(privateKeyPath, rsa.ToXmlString(true));
                SavePublicKey(publicKeyPath, rsa.ToXmlString(false));
            }
        }

        private static void SavePrivateKey(string path, string privateKey)
        {
            var base64 = Convert.ToBase64String(Encoding.UTF8.GetBytes(privateKey));
            var sb = new StringBuilder();
            sb.AppendLine(PrivateKeyHeader);
            sb.AppendLine(base64);
            sb.AppendLine(PrivateKeyFooter);
            File.WriteAllText(path, sb.ToString());
        }

        private static void SavePublicKey(string path, string publicKey)
        {
            var base64 = Convert.ToBase64String(Encoding.UTF8.GetBytes(publicKey));
            var sb = new StringBuilder();
            sb.AppendLine(PublicKeyHeader);
            sb.AppendLine(base64);
            sb.AppendLine(PublicKeyFooter);
            File.WriteAllText(path, sb.ToString());
        }

        public static RSA LoadPrivateKey(string path)
        {
            var content = File.ReadAllText(path)
                .Replace(PrivateKeyHeader, "")
                .Replace(PrivateKeyFooter, "")
                .Trim();

            var pemReader = new PemReader(new StringReader(content));
            var obj = pemReader.ReadObject();
            var privateKeyInfo = (RsaPrivateCrtKeyParameters)obj;

            var rsa = RSA.Create();
            var rsaParameters = new RSAParameters
            {
                Modulus = privateKeyInfo.Modulus.ToByteArrayUnsigned(),
                Exponent = privateKeyInfo.PublicExponent.ToByteArrayUnsigned(),
                D = privateKeyInfo.Exponent.ToByteArrayUnsigned(),
                P = privateKeyInfo.P.ToByteArrayUnsigned(),
                Q = privateKeyInfo.Q.ToByteArrayUnsigned(),
                DP = privateKeyInfo.DP.ToByteArrayUnsigned(),
                DQ = privateKeyInfo.DQ.ToByteArrayUnsigned(),
                InverseQ = privateKeyInfo.QInv.ToByteArrayUnsigned(),
            };
            rsa.ImportParameters(rsaParameters);
            return rsa;
        }

        public static RSA LoadPublicKey(string path)
        {
            var content = File.ReadAllText(path)
                .Replace(PublicKeyHeader, "")
                .Replace(PublicKeyFooter, "")
                .Trim();

            var pemReader = new PemReader(new StringReader(content));
            var obj = pemReader.ReadObject();
            var privateKeyInfo = (RsaPrivateCrtKeyParameters)obj;

            var rsa = RSA.Create();
            var rsaParameters = new RSAParameters
            {
                Modulus = privateKeyInfo.Modulus.ToByteArrayUnsigned(),
                Exponent = privateKeyInfo.PublicExponent.ToByteArrayUnsigned(),
                D = privateKeyInfo.Exponent.ToByteArrayUnsigned(),
                P = privateKeyInfo.P.ToByteArrayUnsigned(),
                Q = privateKeyInfo.Q.ToByteArrayUnsigned(),
                DP = privateKeyInfo.DP.ToByteArrayUnsigned(),
                DQ = privateKeyInfo.DQ.ToByteArrayUnsigned(),
                InverseQ = privateKeyInfo.QInv.ToByteArrayUnsigned(),
            };
            rsa.ImportParameters(rsaParameters);
            return rsa;
        }


        /*public static void CreateKey()
        {
            try
            {
                GenerateAndSaveKeyPair(fullPath, "public.pem");
                Console.WriteLine("密钥对已生成并保存");

                var privateKey = LoadPrivateKey("private.pem");
                var publicKey = LoadPublicKey("public.pem");
                Console.WriteLine("密钥加载成功");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"发生错误: {ex.Message}");
            }
        }*/
    }
}
