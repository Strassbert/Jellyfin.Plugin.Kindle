using System;
using System.IO;

namespace Jellyfin.Plugin.Kindle
{
    /// <summary>
    /// Pure input handling shared by the controllers. Kept off the controllers
    /// themselves so it can be unit tested without an ASP.NET or Jellyfin host.
    /// </summary>
    public static class Validation
    {
        /// <summary>
        /// A deliberately permissive check. Rejecting a deliverable address is worse
        /// than accepting an undeliverable one - the mail server is the real authority,
        /// and the user finds out from the test message.
        /// </summary>
        public static bool IsPlausibleEmailAddress(string? email)
        {
            if (string.IsNullOrWhiteSpace(email) || email.Length > 254)
            {
                return false;
            }

            if (email.Contains(' ', StringComparison.Ordinal))
            {
                return false;
            }

            var at = email.IndexOf('@', StringComparison.Ordinal);
            if (at <= 0 || at != email.LastIndexOf('@'))
            {
                return false;
            }

            var domain = email[(at + 1)..];

            return domain.Length >= 3
                   && domain.Contains('.', StringComparison.Ordinal)
                   && !domain.StartsWith('.')
                   && !domain.EndsWith('.')
                   && !domain.Contains("..", StringComparison.Ordinal);
        }

        /// <summary>
        /// Builds the attachment filename from the library item's name, so the reader
        /// shows the book title rather than whatever the file is called on disk.
        /// </summary>
        /// <remarks>
        /// Sanitisation is not cosmetic: quotes, newlines and path separators in a MIME
        /// filename let a crafted item name break out of the Content-Disposition header.
        /// Item names come from library metadata, which is not necessarily trustworthy.
        /// </remarks>
        public static string BuildAttachmentName(string? itemName, string? extension)
        {
            var name = string.IsNullOrWhiteSpace(itemName) ? "book" : itemName.Trim();

            foreach (var invalid in Path.GetInvalidFileNameChars())
            {
                name = name.Replace(invalid, '_');
            }

            name = name
                .Replace('"', '_')
                .Replace('\'', '_')
                .Replace('\\', '_')
                .Replace('/', '_')
                .Replace('\r', '_')
                .Replace('\n', '_');

            if (name.Length > 100)
            {
                name = name[..100];
            }

            name = name.Trim();

            if (name.Length == 0)
            {
                name = "book";
            }

            return name + (extension ?? string.Empty);
        }
    }
}
