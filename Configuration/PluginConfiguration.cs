using System.Collections.Generic;
using MediaBrowser.Model.Plugins;

namespace Jellyfin.Plugin.Kindle.Configuration
{
    public class PluginConfiguration : BasePluginConfiguration
    {
        // SMTP Settings (Admin)
        public string SmtpHost { get; set; } = string.Empty;
        public int SmtpPort { get; set; } = 587;
        public string SmtpUser { get; set; } = string.Empty;
        public string SmtpPassword { get; set; } = string.Empty;
        public bool UseSsl { get; set; } = true;
        public string SenderEmail { get; set; } = string.Empty;

        // OAuth2 Support (Admin)
        public bool UseOAuth2 { get; set; } = false;
        public string OAuthClientId { get; set; } = string.Empty;
        public string OAuthClientSecret { get; set; } = string.Empty;
        public string OAuthRefreshToken { get; set; } = string.Empty;

        // Per-user Kindle email addresses
        // Key: UserId (string), Value: Kindle email address
        public Dictionary<string, string> UserKindleEmails { get; set; } = new();
    }
}
