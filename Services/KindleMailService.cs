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
    /// <summary>
    /// Outcome of an SMTP operation, so callers can tell a misconfiguration
    /// (actionable by the admin) apart from a transient delivery failure.
    /// </summary>
    public enum MailFailure
    {
        None = 0,
        NotConfigured,
        Connection,
        Authentication,
        Rejected,
        Timeout,
        Unknown
    }

    public sealed record MailResult(bool Success, MailFailure Failure = MailFailure.None, string? Detail = null)
    {
        public static MailResult Ok() => new(true);

        public static MailResult Fail(MailFailure failure, string? detail = null) => new(false, failure, detail);
    }

    public class KindleMailService
    {
        private readonly ILogger<KindleMailService> _logger;
        private static readonly TimeSpan SmtpTimeout = TimeSpan.FromSeconds(30);

        public KindleMailService(ILogger<KindleMailService> logger)
        {
            _logger = logger;
        }

        /// <summary>
        /// Connects and authenticates without sending anything, so the admin page can
        /// verify credentials before a user hits a failing send button.
        /// </summary>
        public async Task<MailResult> TestConnectionAsync(PluginConfiguration config, CancellationToken cancellationToken = default)
        {
            var configError = Validate(config);
            if (configError is not null)
            {
                return MailResult.Fail(MailFailure.NotConfigured, configError);
            }

            using var client = CreateClient();

            try
            {
                await ConnectAndAuthenticateAsync(client, config, cancellationToken).ConfigureAwait(false);
                return MailResult.Ok();
            }
            catch (Exception ex)
            {
                return Classify(ex, "SMTP test");
            }
            finally
            {
                await SafeDisconnectAsync(client).ConfigureAwait(false);
            }
        }

        public async Task<MailResult> SendBookAsync(
            string recipientEmail,
            string filePath,
            string attachmentName,
            PluginConfiguration config,
            CancellationToken cancellationToken = default)
        {
            var configError = Validate(config);
            if (configError is not null)
            {
                return MailResult.Fail(MailFailure.NotConfigured, configError);
            }

            MimeMessage message;
            try
            {
                message = await BuildMessageAsync(recipientEmail, filePath, attachmentName, config, cancellationToken)
                    .ConfigureAwait(false);
            }
            catch (IOException ex)
            {
                _logger.LogError(ex, "[E-Book Share] Could not read {Path} for sending.", filePath);
                return MailResult.Fail(MailFailure.Unknown, "The file could not be read.");
            }
            catch (UnauthorizedAccessException ex)
            {
                _logger.LogError(ex, "[E-Book Share] No permission to read {Path}.", filePath);
                return MailResult.Fail(MailFailure.Unknown, "The file could not be read.");
            }

            using var client = CreateClient();

            try
            {
                await ConnectAndAuthenticateAsync(client, config, cancellationToken).ConfigureAwait(false);
                await client.SendAsync(message, cancellationToken).ConfigureAwait(false);

                _logger.LogInformation("[E-Book Share] '{FileName}' sent to {Email}.", attachmentName, recipientEmail);
                return MailResult.Ok();
            }
            catch (Exception ex)
            {
                return Classify(ex, $"sending '{attachmentName}'");
            }
            finally
            {
                message.Dispose();
                await SafeDisconnectAsync(client).ConfigureAwait(false);
            }
        }

        private static SmtpClient CreateClient() => new() { Timeout = (int)SmtpTimeout.TotalMilliseconds };

        private static string? Validate(PluginConfiguration config)
        {
            if (string.IsNullOrWhiteSpace(config.SmtpHost))
            {
                return "No SMTP host configured.";
            }

            if (config.SmtpPort is <= 0 or > 65535)
            {
                return "SMTP port is out of range.";
            }

            if (string.IsNullOrWhiteSpace(config.SenderEmail) && string.IsNullOrWhiteSpace(config.SmtpUser))
            {
                return "No sender address configured.";
            }

            return null;
        }

        private static async Task ConnectAndAuthenticateAsync(SmtpClient client, PluginConfiguration config, CancellationToken cancellationToken)
        {
            await client.ConnectAsync(config.SmtpHost, config.SmtpPort, ToSocketOptions(config.SecurityMode), cancellationToken)
                .ConfigureAwait(false);

            // An open relay (typically a local postfix) needs no credentials; sending
            // an empty username to one makes it reject the whole session.
            if (!string.IsNullOrWhiteSpace(config.SmtpUser))
            {
                await client.AuthenticateAsync(config.SmtpUser, config.SmtpPassword, cancellationToken).ConfigureAwait(false);
            }
        }

        private static SecureSocketOptions ToSocketOptions(SmtpSecurity security) => security switch
        {
            SmtpSecurity.StartTls => SecureSocketOptions.StartTls,
            SmtpSecurity.SslOnConnect => SecureSocketOptions.SslOnConnect,
            SmtpSecurity.None => SecureSocketOptions.None,
            _ => SecureSocketOptions.Auto
        };

        private static async Task<MimeMessage> BuildMessageAsync(
            string recipientEmail,
            string filePath,
            string attachmentName,
            PluginConfiguration config,
            CancellationToken cancellationToken)
        {
            var senderEmail = string.IsNullOrWhiteSpace(config.SenderEmail) ? config.SmtpUser : config.SenderEmail;
            var senderName = string.IsNullOrWhiteSpace(config.SenderName) ? "Jellyfin" : config.SenderName;

            var message = new MimeMessage();
            message.From.Add(new MailboxAddress(senderName, senderEmail));
            message.To.Add(new MailboxAddress(string.Empty, recipientEmail));

            // Amazon reads the subject as a command: the literal word "convert" asks
            // for conversion to the Kindle format. Anything else is ignored, so a
            // descriptive subject is safe when conversion is not requested.
            message.Subject = config.RequestConversion ? "convert" : attachmentName;

            var bodyBuilder = new BodyBuilder
            {
                TextBody = "Sent from your Jellyfin library."
            };

            // Attach from a stream so the name on the message is the library item's
            // name rather than whatever the file happens to be called on disk.
            await using var fileStream = new FileStream(
                filePath,
                FileMode.Open,
                FileAccess.Read,
                FileShare.Read,
                bufferSize: 81920,
                useAsync: true);

            await bodyBuilder.Attachments.AddAsync(attachmentName, fileStream, cancellationToken).ConfigureAwait(false);
            message.Body = bodyBuilder.ToMessageBody();

            return message;
        }

        private MailResult Classify(Exception ex, string operation)
        {
            switch (ex)
            {
                case AuthenticationException:
                    _logger.LogError(ex, "[E-Book Share] SMTP authentication failed during {Operation}.", operation);
                    return MailResult.Fail(MailFailure.Authentication, ex.Message);

                case SslHandshakeException:
                    _logger.LogError(ex, "[E-Book Share] TLS handshake failed during {Operation}.", operation);
                    return MailResult.Fail(MailFailure.Connection, "TLS handshake failed - check the encryption mode and port.");

                case SmtpCommandException smtp:
                    _logger.LogError(ex, "[E-Book Share] Server rejected the message during {Operation}: {Status}.", operation, smtp.StatusCode);
                    return MailResult.Fail(MailFailure.Rejected, smtp.Message);

                case SmtpProtocolException:
                case System.Net.Sockets.SocketException:
                    _logger.LogError(ex, "[E-Book Share] Could not reach the SMTP server during {Operation}.", operation);
                    return MailResult.Fail(MailFailure.Connection, ex.Message);

                case OperationCanceledException:
                case TimeoutException:
                    _logger.LogError(ex, "[E-Book Share] SMTP timed out during {Operation}.", operation);
                    return MailResult.Fail(MailFailure.Timeout, "The SMTP server did not respond in time.");

                default:
                    _logger.LogError(ex, "[E-Book Share] Unexpected SMTP failure during {Operation}.", operation);
                    return MailResult.Fail(MailFailure.Unknown, ex.Message);
            }
        }

        private async Task SafeDisconnectAsync(SmtpClient client)
        {
            if (!client.IsConnected)
            {
                return;
            }

            try
            {
                // Deliberately not passing the caller's token: a cancelled send must
                // still close the session cleanly instead of leaking the connection.
                await client.DisconnectAsync(true, CancellationToken.None).ConfigureAwait(false);
            }
            catch (Exception ex)
            {
                _logger.LogDebug(ex, "[E-Book Share] Ignoring error while closing the SMTP connection.");
            }
        }
    }
}
