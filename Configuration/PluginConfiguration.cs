using System;
using System.Collections.Generic;
using MediaBrowser.Model.Plugins;

namespace Jellyfin.Plugin.Kindle.Configuration
{
    public class PluginConfiguration : BasePluginConfiguration
    {
        // SMTP Einstellungen (Admin-Bereich)
        public string SmtpHost { get; set; } = string.Empty;
        public int SmtpPort { get; set; } = 587;
        public string SmtpUser { get; set; } = string.Empty;
        public string SmtpPassword { get; set; } = string.Empty;
        public bool UseSsl { get; set; } = true;

        // OAuth2 Support (Admin-Bereich)
        public bool UseOAuth2 { get; set; } = false;
        public string OAuthClientId { get; set; } = string.Empty;
        public string OAuthClientSecret { get; set; } = string.Empty;
        public string OAuthRefreshToken { get; set; } = string.Empty;

        // User-spezifische Kindle E-Mails (Admin-Einsicht möglich)
        // Key: User-Id (String), Value: Kindle-Email-Adresse
        public Dictionary<string, string> UserKindleEmails { get; set; } = new();

        public PluginConfiguration()
        {
        }
    }
}