using System;
using System.Collections.Generic;

namespace Jellyfin.Plugin.Kindle
{
    public static class KindleFormatValidator
    {
        /// <summary>
        /// Formats Amazon's Send-to-Kindle service accepts. MOBI/AZW are legacy: Amazon
        /// stopped accepting them for new documents, but they are left in because other
        /// reader vendors (Kobo, PocketBook, ...) still take them.
        /// </summary>
        private static readonly HashSet<string> AllowedExtensions = new(StringComparer.OrdinalIgnoreCase)
        {
            ".epub", ".pdf", ".txt", ".docx", ".doc",
            ".mobi", ".azw", ".azw3", ".kpf",
            ".rtf", ".htm", ".html",
            ".png", ".jpg", ".jpeg", ".gif", ".bmp"
        };

        /// <summary>
        /// Base64 inflates an attachment by 4/3, plus roughly 1.6% for the mandatory
        /// line breaks. A file that only just fits under the provider's message limit
        /// on disk is therefore rejected by the provider once encoded, which is why the
        /// old bare 50 MB file check let messages through that always bounced.
        /// </summary>
        private const double Base64Overhead = 1.37;

        /// <summary>
        /// Largest file that still fits into a message of <paramref name="maxMessageSizeMb"/>
        /// once encoded.
        /// </summary>
        public static long MaxFileSizeBytes(int maxMessageSizeMb)
        {
            var limit = maxMessageSizeMb > 0 ? maxMessageSizeMb : 50;
            return (long)(limit * 1024L * 1024L / Base64Overhead);
        }

        public static IReadOnlyCollection<string> SupportedExtensions => AllowedExtensions;

        public static bool IsCompatible(string? extension)
        {
            if (string.IsNullOrEmpty(extension))
            {
                return false;
            }

            var ext = extension.StartsWith('.') ? extension : "." + extension;
            return AllowedExtensions.Contains(ext);
        }
    }
}
