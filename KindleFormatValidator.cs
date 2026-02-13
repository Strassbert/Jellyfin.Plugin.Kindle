using System;
using System.Collections.Generic;

namespace Jellyfin.Plugin.Kindle
{
    public static class KindleFormatValidator
    {
        // Erlaubte Formate basierend auf Amazons aktuellen Richtlinien
        private static readonly HashSet<string> AllowedExtensions = new(StringComparer.OrdinalIgnoreCase)
        {
            ".epub", ".pdf", ".txt", ".docx"
        };

        public static bool IsCompatible(string extension)
        {
            if (string.IsNullOrEmpty(extension)) return false;
            
            // Punkt entfernen falls vorhanden (z.B. ".epub" -> "epub")
            var ext = extension.StartsWith('.') ? extension : "." + extension;
            return AllowedExtensions.Contains(ext);
        }
    }
}