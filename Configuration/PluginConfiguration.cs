using System;
using System.Collections.Generic;
using System.Text.Json;
using System.Xml.Serialization;
using MediaBrowser.Model.Plugins;

namespace Jellyfin.Plugin.Kindle.Configuration
{
    /// <summary>
    /// How the SMTP connection is secured. Replaces the old <c>UseSsl</c> flag, which
    /// only ever chose between STARTTLS and "Auto" and therefore could not describe
    /// implicit TLS on port 465 - the most common provider setup after 587.
    /// </summary>
    public enum SmtpSecurity
    {
        /// <summary>Let MailKit decide from the port (465 =&gt; implicit TLS, otherwise STARTTLS when offered).</summary>
        Auto = 0,

        /// <summary>Connect in the clear, then require STARTTLS. Typical for port 587.</summary>
        StartTls = 1,

        /// <summary>TLS from the first byte. Typical for port 465.</summary>
        SslOnConnect = 2,

        /// <summary>No transport encryption. Only sensible for a relay on localhost.</summary>
        None = 3
    }

    public class PluginConfiguration : BasePluginConfiguration
    {
        private const int CurrentConfigVersion = 1;

        private readonly object _emailLock = new();

        // Bumped by Migrate() once an older configuration has been upgraded in place.
        public int ConfigVersion { get; set; }

        // SMTP settings (admin)
        public string SmtpHost { get; set; } = string.Empty;
        public int SmtpPort { get; set; } = 587;
        public string SmtpUser { get; set; } = string.Empty;
        public string SmtpPassword { get; set; } = string.Empty;
        public string SenderEmail { get; set; } = string.Empty;
        public string SenderName { get; set; } = "Jellyfin";

        /// <summary>
        /// Backing value for <see cref="SecurityMode"/>, stored as an int on purpose.
        /// Jellyfin's API serializer registers JsonStringEnumConverter, so an enum
        /// property would arrive at the configuration page as "Auto" rather than 0 and
        /// the &lt;select&gt; could not match it back.
        /// </summary>
        public int Security { get; set; } = (int)SmtpSecurity.Auto;

        [XmlIgnore]
        public SmtpSecurity SecurityMode =>
            Enum.IsDefined(typeof(SmtpSecurity), Security) ? (SmtpSecurity)Security : SmtpSecurity.Auto;

        /// <summary>
        /// Legacy flag, kept so existing plugin XML still deserializes. <see cref="Migrate"/>
        /// folds it into <see cref="Security"/>; nothing reads it afterwards.
        /// </summary>
        public bool UseSsl { get; set; } = true;

        /// <summary>
        /// Ask Amazon to convert the attachment to the Kindle format by sending
        /// "convert" as the mail subject.
        /// </summary>
        public bool RequestConversion { get; set; }

        /// <summary>
        /// Provider-side limit for the whole message in MB. Amazon's Send-to-Kindle
        /// caps at 50 MB; the effective file limit is lower because attachments are
        /// base64 encoded (see KindleFormatValidator.MaxFileSizeBytes).
        /// </summary>
        public int MaxMessageSizeMb { get; set; } = 50;

        // Legacy OAuth2 fields. The old implementation passed the refresh token
        // straight to SASL XOAUTH2 as if it were an access token and never used the
        // client id/secret at all, so it could not have worked. Kept only so existing
        // configuration files still deserialize without data loss.
        [Obsolete("OAuth2 was never functional. Use an app password over SMTP instead.")]
        public bool UseOAuth2 { get; set; }

        [Obsolete("OAuth2 was never functional. Use an app password over SMTP instead.")]
        public string OAuthClientId { get; set; } = string.Empty;

        [Obsolete("OAuth2 was never functional. Use an app password over SMTP instead.")]
        public string OAuthClientSecret { get; set; } = string.Empty;

        [Obsolete("OAuth2 was never functional. Use an app password over SMTP instead.")]
        public string OAuthRefreshToken { get; set; } = string.Empty;

        /// <summary>
        /// Per-user reader addresses. Stored as a JSON string because XmlSerializer
        /// cannot serialize a Dictionary.
        /// </summary>
        public string UserKindleEmailsJson { get; set; } = "{}";

        [XmlIgnore]
        public Dictionary<string, string> UserKindleEmails
        {
            get => ReadEmails();
            set => UserKindleEmailsJson = JsonSerializer.Serialize(value);
        }

        /// <summary>
        /// Reads the stored address for a user. Returns <c>null</c> when none is set.
        /// </summary>
        public string? GetUserEmail(string userId)
        {
            lock (_emailLock)
            {
                return ReadEmails().TryGetValue(userId, out var email) && !string.IsNullOrWhiteSpace(email)
                    ? email
                    : null;
            }
        }

        /// <summary>
        /// Adds, replaces or (when <paramref name="email"/> is null) removes a user's
        /// address. Read-modify-write is serialised so two users saving at the same
        /// time cannot drop each other's entry.
        /// </summary>
        public void SetUserEmail(string userId, string? email)
        {
            lock (_emailLock)
            {
                var emails = ReadEmails();

                if (string.IsNullOrWhiteSpace(email))
                {
                    emails.Remove(userId);
                }
                else
                {
                    emails[userId] = email.Trim();
                }

                UserKindleEmailsJson = JsonSerializer.Serialize(emails);
            }
        }

        /// <summary>
        /// Upgrades a configuration written by an older plugin version. Safe to call
        /// on every startup.
        /// </summary>
        public void Migrate()
        {
            if (ConfigVersion >= CurrentConfigVersion)
            {
                return;
            }

            // Old behaviour was `UseSsl ? StartTls : Auto`, which silently failed on
            // port 465 because implicit TLS was unreachable. Preserve the user's
            // intent while routing 465 to the mode that actually works there.
            // A fresh install has nothing to migrate; leave the declared default
            // (Auto) alone instead of deriving a mode from unset values.
            if (!string.IsNullOrWhiteSpace(SmtpHost))
            {
                Security = (int)(UseSsl
                    ? (SmtpPort == 465 ? SmtpSecurity.SslOnConnect : SmtpSecurity.StartTls)
                    : SmtpSecurity.Auto);
            }

            ConfigVersion = CurrentConfigVersion;
        }

        private Dictionary<string, string> ReadEmails()
        {
            if (string.IsNullOrWhiteSpace(UserKindleEmailsJson))
            {
                return new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase);
            }

            try
            {
                return JsonSerializer.Deserialize<Dictionary<string, string>>(UserKindleEmailsJson)
                       ?? new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase);
            }
            catch (JsonException)
            {
                // A hand-edited or truncated configuration file must not take the
                // whole controller down; treat it as "no addresses configured".
                return new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase);
            }
        }
    }
}
