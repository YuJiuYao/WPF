using System;
using System.Security.Cryptography;
using System.Text;

namespace AnBiaoZhiJianTong.Common.Utilities
{
    public static class KeyEncryption
    {
        //密钥Key和Iv
        public static string Key = "0123456789ABCDEF0123456789ABCDEF";  // 32个 ASCII 字符（32 Byte）
        public static string Iv = "ABCDEF0123456789";  // 16个 ASCII 字符（16 Byte）
        //加密方法
        public static string EncryptAes(string plainText, string key, string iv)
        {
            Aes aes = Aes.Create();
            aes.Key = Encoding.UTF8.GetBytes(key);  // 32 bytes for AES-256
            aes.IV = Encoding.UTF8.GetBytes(iv);    // 16 bytes (128 bit)
            ICryptoTransform encryptor = aes.CreateEncryptor();
            byte[] encryptedBytes = encryptor.TransformFinalBlock(Encoding.UTF8.GetBytes(plainText), 0, plainText.Length);
            return Convert.ToBase64String(encryptedBytes);
        }
        //解密方法
        public static string DecryptAes(string encryptedText, string key = "0123456789ABCDEF0123456789ABCDEF", string iv = "ABCDEF0123456789")
        {
            byte[] keyBytes = new byte[32];
            byte[] ivBytes = new byte[16];
            Array.Copy(Encoding.UTF8.GetBytes(key), keyBytes, Math.Min(keyBytes.Length, Encoding.UTF8.GetBytes(key).Length));
            Array.Copy(Encoding.UTF8.GetBytes(iv), ivBytes, Math.Min(ivBytes.Length, Encoding.UTF8.GetBytes(iv).Length));

            using (Aes aes = Aes.Create())
            {
                aes.Key = keyBytes;
                aes.IV = ivBytes;
                ICryptoTransform decryptor = aes.CreateDecryptor();
                byte[] encryptedBytes = Convert.FromBase64String(encryptedText);
                byte[] decryptedBytes = decryptor.TransformFinalBlock(encryptedBytes, 0, encryptedBytes.Length);
                return Encoding.UTF8.GetString(decryptedBytes);
            }
        }
        public static string GenerateKey()
        {
            byte[] key = new byte[32];
            var rng = new RNGCryptoServiceProvider();
            rng.GetBytes(key);
            return Convert.ToBase64String(key).TrimEnd('=');
        }
        public static string GenerateIv()
        {
            byte[] iv = new byte[16];  // 16 Bytes for AES
            var rng = new RNGCryptoServiceProvider();
            rng.GetBytes(iv); 
            return Convert.ToBase64String(iv).TrimEnd('=');  
        }
    }
}
