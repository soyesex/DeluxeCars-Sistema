using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;

namespace DeluxeCarsDesktop.Utils
{
    public static class EncryptionHelper
    {
        private static readonly byte[] s_entropy = new byte[] { 1, 2, 3, 4, 5, 10, 15, 20 };

        public static byte[]? Encrypt(string? data)
        {
            if (string.IsNullOrEmpty(data)) return null;

            var dataBytes = Encoding.UTF8.GetBytes(data);
            return ProtectedData.Protect(dataBytes, s_entropy, DataProtectionScope.CurrentUser);
        }

        public static string? Decrypt(byte[]? data)
        {
            if (data == null || data.Length == 0) return null;

            try
            {
                var decryptedBytes = ProtectedData.Unprotect(data, s_entropy, DataProtectionScope.CurrentUser);
                return Encoding.UTF8.GetString(decryptedBytes);
            }
            catch (CryptographicException)
            {
                return null;
            }
        }
    }
}
