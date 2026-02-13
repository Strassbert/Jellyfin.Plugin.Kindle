using System;
using System.IO;
using System.Threading;
using System.Threading.Tasks;
using MailKit.Net.Smtp;
using MailKit.Security;
using MimeKit;
using Jellyfin.Plugin.Kindle.Configuration;
using Microsoft.Extensions.Logging;

namespace Jellyfin.Plugin.Kindle.Services
{
    public class KindleMailService
    {
        private readonly ILogger<KindleMailService> _logger;

        public KindleMailService(ILogger<KindleMailService> logger)
        {
            _logger = logger;
        }

        public async Task SendBookAsync(
            string recipientEmail, 
            string filePath, 
            string fileName, 
            PluginConfiguration config)
        {
            var message = new MimeMessage();
            message.From.Add(new MailboxAddress("Jellyfin Kindle", config.SmtpUser));
            message.To.Add(new MailboxAddress("Kindle", recipientEmail));
            message.Subject = $"Book: {fileName}";

            var bodyBuilder = new BodyBuilder
            {
                TextBody = "Hier ist dein angefordertes Buch von Jellyfin."
            };

            // E-Book als Attachment hinzufügen
            bodyBuilder.Attachments.Add(filePath);
            message.Body = bodyBuilder.ToMessageBody();

            using var client = new SmtpClient();
            try
            {
                // Verbindung herstellen
                var secureOption = config.UseSsl ? SecureSocketOptions.StartTls : SecureSocketOptions.Auto;
                await client.ConnectAsync(config.SmtpHost, config.SmtpPort, secureOption);

                // Authentifizierung (Normal oder OAuth2)
                if (config.UseOAuth2)
                {
                    await AuthenticateOAuth2(client, config);
                }
                else
                {
                    await client.AuthenticateAsync(config.SmtpUser, config.SmtpPassword);
                }

                await client.SendAsync(message);
                _logger.LogInformation("Buch erfolgreich an {Email} gesendet.", recipientEmail);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Fehler beim Senden der E-Mail an Kindle.");
                throw;
            }
            finally
            {
                await client.DisconnectAsync(true);
            }
        }

        private async Task AuthenticateOAuth2(SmtpClient client, PluginConfiguration config)
        {
            // Hinweis für 10.11.X: Wir nutzen hier den SASL XOAUTH2 Mechanismus
            // In einer vollen Implementierung müsste hier der Refresh-Token-Logik stehen
            var oauth2 = new SaslMechanismOAuth2(config.SmtpUser, config.OAuthRefreshToken);
            await client.AuthenticateAsync(oauth2);
        }
    }
}