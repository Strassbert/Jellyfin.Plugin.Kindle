using System;
using System.Text;
using System.Security.Cryptography;

namespace Jellyfin.Plugin.Kindle.Services
{
    /// <summary>
    /// Service for handling password encryption and decryption
    /// </summary>
    public class KindleSecurityService
    {
        private const string EncryptionPrefix = "ENCRYPTED:";

        /// <summary>
        /// Encrypt a password using DPAPI (Data Protection API)
        /// DPAPI automatically uses the current user/machine context for encryption
        /// </summary>
        public string EncryptPassword(string plainPassword)
        {
            if (string.IsNullOrEmpty(plainPassword))
                return plainPassword;

            // If already encrypted, return as-is
            if (plainPassword.StartsWith(EncryptionPrefix))
                return plainPassword;

            try
            {
                var plainBytes = Encoding.UTF8.GetBytes(plainPassword);
                var encryptedBytes = ProtectedData.Protect(
                    plainBytes,
                    null, // No additional entropy
                    DataProtectionScope.CurrentUser);

                var base64 = Convert.ToBase64String(encryptedBytes);
                return EncryptionPrefix + base64;
            }
            catch (Exception ex)
            {
                // If encryption fails, return original (will fail at runtime with clear error)
                System.Diagnostics.Debug.WriteLine($"Password encryption failed: {ex.Message}");
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
                var base64 = encryptedPassword.Substring(EncryptionPrefix.Length);
                var encryptedBytes = Convert.FromBase64String(base64);
                var plainBytes = ProtectedData.Unprotect(
                    encryptedBytes,
                    null,
                    DataProtectionScope.CurrentUser);

                return Encoding.UTF8.GetString(plainBytes);
            }
            catch (Exception ex)
            {
                // If decryption fails, return empty (better than corrupted password)
                System.Diagnostics.Debug.WriteLine($"Password decryption failed: {ex.Message}");
                return string.Empty;
            }
        }

        /// <summary>
        /// Check if a password is encrypted
        /// </summary>
        public bool IsEncrypted(string password)
        {
            return !string.IsNullOrEmpty(password) && password.StartsWith(EncryptionPrefix);
        }
    }
}
