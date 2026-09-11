using System;
using System.Security.Cryptography;
using System.Text;

namespace HelperSharedLibrary
{
    public static class SecurityHelper
    {
        public static string DeriveKey(string partA, string partB)
        {
            using (SHA256 sha256 = SHA256.Create())
            {
                string combinedString = partA + partB;
                byte[] hash = sha256.ComputeHash(Encoding.UTF8.GetBytes(combinedString));
                return Convert.ToBase64String(hash);
            }
        }

        public static string DeriveIV(string partA, string partB)
        {
            using (SHA256 sha256 = SHA256.Create())
            {
                string combinedString = "IV-" + partA + partB;
                byte[] hash = sha256.ComputeHash(Encoding.UTF8.GetBytes(combinedString));
                Array.Resize(ref hash, 16);
                return Convert.ToBase64String(hash);
            }
        }

        public static SecurityData EncryptData(string data, string derivedKey, string derivedIV = null)
        {
            using (Aes aes = Aes.Create())
            {
                aes.Key = Convert.FromBase64String(derivedKey);

                if (!string.IsNullOrEmpty(derivedIV))
                    aes.IV = Convert.FromBase64String(derivedIV);
                else
                    aes.GenerateIV();

                using (ICryptoTransform encryptor = aes.CreateEncryptor(aes.Key, aes.IV))
                {
                    byte[] utf8Bytes = Encoding.UTF8.GetBytes(data);
                    byte[] encryptedData = encryptor.TransformFinalBlock(utf8Bytes, 0, utf8Bytes.Length);

                    return new SecurityData(
                        Convert.ToBase64String(encryptedData),
                        Convert.ToBase64String(aes.IV)
                    );
                }
            }
        }

        public static string DecryptData(SecurityData securityData, string derivedKey)
        {
            using (Aes aes = Aes.Create())
            {
                aes.Key = Convert.FromBase64String(derivedKey);
                aes.IV = Convert.FromBase64String(securityData.ivBase64);

                using (ICryptoTransform decryptor = aes.CreateDecryptor(aes.Key, aes.IV))
                {
                    byte[] encryptedBytes = Convert.FromBase64String(securityData.encryptedData);
                    byte[] decryptedData = decryptor.TransformFinalBlock(encryptedBytes, 0, encryptedBytes.Length);
                    return Encoding.UTF8.GetString(decryptedData);
                }
            }
        }
    }
}
