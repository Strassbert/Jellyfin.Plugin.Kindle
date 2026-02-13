using System;
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
        private static readonly TimeSpan SmtpTimeout = TimeSpan.FromSeconds(30);

        public KindleMailService(ILogger<KindleMailService> logger)
        {
            _logger = logger;
        }

        public async Task SendBookAsync(
            string recipientEmail,
            string filePath,
            string fileName,
            PluginConfiguration config,
            CancellationToken cancellationToken = default)
        {
            var senderEmail = string.IsNullOrWhiteSpace(config.SenderEmail) ? config.SmtpUser : config.SenderEmail;

            var message = new MimeMessage();
            message.From.Add(new MailboxAddress("Jellyfin Kindle", senderEmail));
            message.To.Add(new MailboxAddress("Kindle", recipientEmail));
            message.Subject = $"Book: {fileName}";

            var bodyBuilder = new BodyBuilder
            {
                TextBody = "Your requested book from Jellyfin."
            };

            bodyBuilder.Attachments.Add(filePath);
            message.Body = bodyBuilder.ToMessageBody();

            using var client = new SmtpClient();
            client.Timeout = (int)SmtpTimeout.TotalMilliseconds;

            try
            {
                var secureOption = config.UseSsl ? SecureSocketOptions.StartTls : SecureSocketOptions.Auto;
                await client.ConnectAsync(config.SmtpHost, config.SmtpPort, secureOption, cancellationToken);

                if (config.UseOAuth2)
                {
                    await AuthenticateOAuth2Async(client, config, cancellationToken);
                }
                else
                {
                    await client.AuthenticateAsync(config.SmtpUser, config.SmtpPassword, cancellationToken);
                }

                await client.SendAsync(message, cancellationToken);
                _logger.LogInformation("Book '{FileName}' sent to {Email}.", fileName, recipientEmail);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to send email to {Email}.", recipientEmail);
                throw;
            }
            finally
            {
                if (client.IsConnected)
                {
                    await client.DisconnectAsync(true, cancellationToken);
                }
            }
        }

        private static async Task AuthenticateOAuth2Async(SmtpClient client, PluginConfiguration config, CancellationToken cancellationToken)
        {
            var oauth2 = new SaslMechanismOAuth2(config.SmtpUser, config.OAuthRefreshToken);
            await client.AuthenticateAsync(oauth2, cancellationToken);
        }
    }
}
