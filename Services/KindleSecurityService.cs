using System;
using System.Text;
using System.Runtime.InteropServices;
#if NET5_0_OR_GREATER && WINDOWS
using System.Security.Cryptography;
#endif

namespace Jellyfin.Plugin.Kindle.Services
{
    /// <summary>
    /// Service for handling password encryption and decryption
    /// </summary>
    public class KindleSecurityService
    {
        private const string EncryptionPrefix = "ENCRYPTED:";

        /// <summary>
        /// Encrypt a password using DPAPI (Data Protection API) on Windows
        /// DPAPI automatically uses the current user/machine context for encryption
        /// </summary>
        public string EncryptPassword(string plainPassword)
        {
            if (string.IsNullOrEmpty(plainPassword))
                return plainPassword;

            // If already encrypted, return as-is
            if (plainPassword.StartsWith(EncryptionPrefix))
                return plainPassword;

            // Only encrypt on Windows where DPAPI is available
            if (!RuntimeInformation.IsOSPlatform(OSPlatform.Windows))
                return plainPassword;

            try
            {
#if NET5_0_OR_GREATER && WINDOWS
                var plainBytes = Encoding.UTF8.GetBytes(plainPassword);
                var encryptedBytes = ProtectedData.Protect(
                    plainBytes,
                    null, // No additional entropy
                    DataProtectionScope.CurrentUser);

                var base64 = Convert.ToBase64String(encryptedBytes);
                return EncryptionPrefix + base64;
#else
                return plainPassword;
#endif
            }
            catch
            {
                // If encryption fails, return original (will fail at runtime with clear error)
                return plainPassword;
            }
        }

        /// <summary>
        /// Decrypt a password using DPAPI
        /// </summary>
        public string DecryptPassword(string encryptedPassword)
        {
            if (string.IsNullOrEmpty(encryptedPassword))
                return encryptedPassword;

            // If not encrypted format, return as-is (for backward compatibility)
            if (!encryptedPassword.StartsWith(EncryptionPrefix))
                return encryptedPassword;

            try
            {
#if NET5_0_OR_GREATER && WINDOWS
                var base64 = encryptedPassword.Substring(EncryptionPrefix.Length);
                var encryptedBytes = Convert.FromBase64String(base64);
                var plainBytes = ProtectedData.Unprotect(
                    encryptedBytes,
                    null,
                    DataProtectionScope.CurrentUser);

                return Encoding.UTF8.GetString(plainBytes);
#else
                return encryptedPassword;
#endif
            }
            catch
            {
                // If decryption fails, return empty (better than corrupted password)
                return string.Empty;
            }
        }
    }
}
