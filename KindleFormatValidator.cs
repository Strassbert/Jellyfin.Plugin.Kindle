using System;
using System.Collections.Generic;

namespace Jellyfin.Plugin.Kindle
{
    public static class KindleFormatValidator
    {
        // Supported formats based on Amazon's current guidelines
        private static readonly HashSet<string> AllowedExtensions = new(StringComparer.OrdinalIgnoreCase)
        {
            ".epub", ".pdf", ".txt", ".docx", ".doc",
            ".mobi", ".azw", ".azw3", ".kpf",
            ".rtf", ".htm", ".html",
            ".png", ".jpg", ".jpeg", ".gif", ".bmp"
        };

        public static bool IsCompatible(string? extension)
        {
            if (string.IsNullOrEmpty(extension)) return false;

            var ext = extension.StartsWith('.') ? extension : "." + extension;
            return AllowedExtensions.Contains(ext);
        }
    }
}
