using System.Collections.Generic;
using System.Text.Json;
using System.Xml.Serialization;
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

        // Per-user Kindle email addresses stored as JSON string (XmlSerializer cannot handle Dictionary)
        public string UserKindleEmailsJson { get; set; } = "{}";

        [XmlIgnore]
        public Dictionary<string, string> UserKindleEmails
        {
            get => JsonSerializer.Deserialize<Dictionary<string, string>>(UserKindleEmailsJson ?? "{}") ?? new();
            set => UserKindleEmailsJson = JsonSerializer.Serialize(value);
        }
    }
}
