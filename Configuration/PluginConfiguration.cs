using System;
using System.Collections.Generic;
using System.Text.Json;
using System.Xml.Serialization;
using Jellyfin.Plugin.Kindle.Models;
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

        // === LEGACY: Per-user Kindle email addresses (backward compatibility) ===
        public string UserKindleEmailsJson { get; set; } = "{}";

        [XmlIgnore]
        public Dictionary<string, string> UserKindleEmails
        {
            get => JsonSerializer.Deserialize<Dictionary<string, string>>(UserKindleEmailsJson ?? "{}") ?? new();
            set => UserKindleEmailsJson = JsonSerializer.Serialize(value);
        }

        // === NEW: Multiple devices per user ===
        /// <summary>
        /// User devices stored as JSON: { userId: [UserDevice[], ...] }
        /// </summary>
        public string UserDevicesJson { get; set; } = "{}";

        [XmlIgnore]
        public Dictionary<string, List<UserDevice>> UserDevices
        {
            get
            {
                try
                {
                    return JsonSerializer.Deserialize<Dictionary<string, List<UserDevice>>>(UserDevicesJson ?? "{}") ?? new();
                }
                catch
                {
                    return new();
                }
            }
            set => UserDevicesJson = JsonSerializer.Serialize(value ?? new());
        }

        // === NEW: Send history and statistics ===
        /// <summary>
        /// Send history logs stored as JSON array
        /// </summary>
        public string SendLogsJson { get; set; } = "[]";

        [XmlIgnore]
        public List<SendLog> SendLogs
        {
            get
            {
                try
                {
                    return JsonSerializer.Deserialize<List<SendLog>>(SendLogsJson ?? "[]") ?? new();
                }
                catch
                {
                    return new();
                }
            }
            set => SendLogsJson = JsonSerializer.Serialize(value ?? new());
        }

        // === Feature Flags ===
        /// <summary>
        /// Enable multiple devices per user
        /// </summary>
        public bool EnableMultipleDevices { get; set; } = true;

        /// <summary>
        /// Enable send history tracking
        /// </summary>
        public bool EnableSendHistory { get; set; } = true;

        /// <summary>
        /// Maximum number of send logs to keep (0 = unlimited)
        /// </summary>
        public int MaxSendLogs { get; set; } = 1000;
    }
}
