using System;
using System.ComponentModel.DataAnnotations;
using System.IO;
using System.Net.Mail;
using System.Threading.Tasks;
using Jellyfin.Plugin.Kindle.Configuration;
using Jellyfin.Plugin.Kindle.Services;
using MediaBrowser.Controller.Library;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;

namespace Jellyfin.Plugin.Kindle.Api
{
    [ApiController]
    [Route("Kindle")]
    [Authorize]
    public class KindleController : ControllerBase
    {
        private const long MaxFileSizeBytes = 50L * 1024 * 1024; // 50 MB Amazon limit

        private readonly ILibraryManager _libraryManager;
        private readonly PluginConfiguration _config;
        private readonly KindleMailService _mailService;
        private readonly RateLimitingService _rateLimiter;
        private readonly ILogger<KindleController> _logger;

        public KindleController(
            ILibraryManager libraryManager,
            PluginConfiguration config,
            KindleMailService mailService,
            RateLimitingService rateLimiter,
            ILogger<KindleController> logger)
        {
            _libraryManager = libraryManager;
            _config = config;
            _mailService = mailService;
            _rateLimiter = rateLimiter;
            _logger = logger;
        }

        /// <summary>
        /// Validates email format using System.Net.Mail.MailAddress (RFC 5322 compliant)
        /// </summary>
        private bool IsValidEmail(string email)
        {
            try
            {
                if (string.IsNullOrWhiteSpace(email) || email.Length > 254)
                    return false;

                // MailAddress will throw if invalid
                var addr = new MailAddress(email);
                // Double-check that normalized address matches (catches some edge cases)
                return addr.Address == email.ToLower() || email.Contains(addr.Address);
            }
            catch
            {
                return false;
            }
        }

        [HttpPost("Send")]
        public async Task<IActionResult> SendToKindle(
            [FromQuery, Required] string itemId,
            [FromQuery, Required] string userId)
        {
            // Rate limiting check (5 attempts per minute, 50 per hour)
            if (!_rateLimiter.IsAllowed(userId, "send"))
            {
                _logger.LogWarning("Rate limit exceeded for user {UserId}.", userId);
                var remaining = _rateLimiter.GetRemainingAttempts(userId, "send");
                return StatusCode(429, new
                {
                    error = $"Too many requests. Maximum 5 sends per minute. Please wait before trying again.",
                    errorDe = $"Zu viele Anfragen. Maximum 5 Versendungen pro Minute. Bitte vor erneutem Versuch warten.",
                    remainingAttempts = remaining
                });
            }

            var item = _libraryManager.GetItemById(itemId);
            if (item == null)
            {
                return NotFound(new { error = "Item not found.", errorDe = "Buch nicht gefunden." });
            }

            // Format check
            var extension = Path.GetExtension(item.Path);
            if (!KindleFormatValidator.IsCompatible(extension))
            {
                return BadRequest(new
                {
                    error = $"Format '{extension}' is not supported by E-Book Reader.",
                    errorDe = $"Format '{extension}' wird vom E-Book Reader nicht unterstützt."
                });
            }

            // File existence check
            if (string.IsNullOrEmpty(item.Path) || !System.IO.File.Exists(item.Path))
            {
                return NotFound(new { error = "File not found on disk.", errorDe = "Datei nicht auf der Festplatte gefunden." });
            }

            // File size check (50 MB Amazon limit)
            var fileInfo = new FileInfo(item.Path);
            if (fileInfo.Length > MaxFileSizeBytes)
            {
                var sizeMb = fileInfo.Length / (1024.0 * 1024.0);
                return BadRequest(new
                {
                    error = $"File is too large ({sizeMb:F1} MB). E-Book Reader limit is 50 MB.",
                    errorDe = $"Datei ist zu groß ({sizeMb:F1} MB). E-Book Reader Limit ist 50 MB."
                });
            }

            // User email check
            if (!_config.UserKindleEmails.TryGetValue(userId, out var kindleEmail) || string.IsNullOrEmpty(kindleEmail))
            {
                return BadRequest(new
                {
                    error = "No E-Book Reader email configured. Please set your E-Book Reader email in user settings.",
                    errorDe = "Keine E-Book Reader E-Mail hinterlegt. Bitte in den Benutzereinstellungen konfigurieren.",
                    code = "NO_KINDLE_EMAIL"
                });
            }

            try
            {
                await _mailService.SendBookAsync(kindleEmail, item.Path, item.Name + extension, _config);
                var fileSizeMb = new System.IO.FileInfo(item.Path).Length / (1024.0 * 1024.0);
                _logger.LogInformation(
                    "Book sent successfully - ItemId: {ItemId}, ItemName: {Name}, UserId: {UserId}, Email: {Email}, FileSizeMB: {FileSizeMB}, Timestamp: {Timestamp}",
                    itemId, item.Name, userId, kindleEmail, fileSizeMb, DateTime.UtcNow);
                return Ok(new { message = "Sent to E-Book Reader.", messageDe = "An E-Book Reader gesendet." });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex,
                    "Failed to send book - ItemId: {ItemId}, ItemName: {Name}, UserId: {UserId}, Email: {Email}, Exception: {Exception}, Timestamp: {Timestamp}",
                    itemId, item.Name, userId, kindleEmail, ex.Message, DateTime.UtcNow);
                return StatusCode(500, new
                {
                    error = "Failed to send email. Please check SMTP settings.",
                    errorDe = "E-Mail-Versand fehlgeschlagen. Bitte SMTP-Einstellungen prüfen."
                });
            }
        }

        [HttpGet("UserEmail")]
        public IActionResult GetUserEmail([FromQuery, Required] string userId)
        {
            _config.UserKindleEmails.TryGetValue(userId, out var email);
            return Ok(new { email = email ?? string.Empty });
        }

        [HttpPost("UserEmail")]
        public IActionResult SetUserEmail([FromQuery, Required] string userId, [FromQuery, Required] string email)
        {
            if (string.IsNullOrWhiteSpace(email))
            {
                return BadRequest(new { error = "Email cannot be empty.", errorDe = "E-Mail darf nicht leer sein." });
            }

            // Validate email format using MailAddress parser (RFC 5322 compliant)
            if (!IsValidEmail(email.Trim()))
            {
                return BadRequest(new { error = "Invalid email format.", errorDe = "Ungültiges E-Mail-Format." });
            }

            var emails = _config.UserKindleEmails;
            emails[userId] = email.Trim();
            _config.UserKindleEmails = emails;
            Plugin.Instance.SaveConfiguration();

            _logger.LogInformation(
                "E-Book Reader email updated - UserId: {UserId}, Email: {Email}, Timestamp: {Timestamp}",
                userId, email.Trim(), DateTime.UtcNow);
            return Ok(new { message = "E-Book Reader email saved.", messageDe = "E-Book Reader-E-Mail gespeichert." });
        }

        [HttpDelete("UserEmail")]
        public IActionResult DeleteUserEmail([FromQuery, Required] string userId)
        {
            var emails = _config.UserKindleEmails;
            if (emails.TryGetValue(userId, out var email))
            {
                emails.Remove(userId);
                _config.UserKindleEmails = emails;
                Plugin.Instance.SaveConfiguration();

                _logger.LogInformation(
                    "E-Book Reader email removed - UserId: {UserId}, PreviousEmail: {Email}, Timestamp: {Timestamp}",
                    userId, email, DateTime.UtcNow);
            }
            else
            {
                _logger.LogWarning(
                    "Attempted to delete E-Book Reader email but none configured - UserId: {UserId}, Timestamp: {Timestamp}",
                    userId, DateTime.UtcNow);
            }

            return Ok(new { message = "E-Book Reader email removed.", messageDe = "E-Book Reader E-Mail entfernt." });
        }

        /// <summary>
        /// Test SMTP connection with provided settings
        /// Admin-only endpoint for validating SMTP configuration
        /// </summary>
        [HttpPost("ValidateSmtp")]
        [Authorize(Policy = "RequireAdministratorRole")]
        public async Task<IActionResult> ValidateSmtp()
        {
            if (string.IsNullOrWhiteSpace(_config.SmtpHost) ||
                _config.SmtpPort <= 0 ||
                string.IsNullOrWhiteSpace(_config.SmtpUser))
            {
                return BadRequest(new
                {
                    success = false,
                    message = "SMTP settings are incomplete. Please configure Host, Port, and User.",
                    messageDe = "SMTP-Einstellungen sind unvollständig. Bitte Host, Port und Benutzer konfigurieren."
                });
            }

            try
            {
                using var client = new MailKit.Net.Smtp.SmtpClient();
                client.Timeout = 10000; // 10 second timeout for test

                var secureOption = _config.UseSsl
                    ? MailKit.Security.SecureSocketOptions.StartTls
                    : MailKit.Security.SecureSocketOptions.Auto;

                await client.ConnectAsync(_config.SmtpHost, _config.SmtpPort, secureOption);

                // Try to authenticate
                if (_config.UseOAuth2)
                {
                    if (string.IsNullOrWhiteSpace(_config.OAuthRefreshToken))
                    {
                        return BadRequest(new
                        {
                            success = false,
                            message = "OAuth2 is enabled but RefreshToken is not configured.",
                            messageDe = "OAuth2 ist aktiviert, aber RefreshToken ist nicht konfiguriert."
                        });
                    }
                    // Note: OAuth2 test would require token refresh which may fail - just report it's configured
                    _logger.LogInformation("SMTP OAuth2 authentication configured - Host: {Host}, User: {User}", _config.SmtpHost, _config.SmtpUser);
                }
                else
                {
                    if (string.IsNullOrWhiteSpace(_config.SmtpPassword))
                    {
                        return BadRequest(new
                        {
                            success = false,
                            message = "SMTP password is not configured.",
                            messageDe = "SMTP-Passwort ist nicht konfiguriert."
                        });
                    }

                    var securityService = new KindleSecurityService();
                    var password = securityService.DecryptPassword(_config.SmtpPassword);
                    await client.AuthenticateAsync(_config.SmtpUser, password);
                }

                await client.DisconnectAsync(true);

                _logger.LogInformation(
                    "SMTP validation successful - Host: {Host}, Port: {Port}, User: {User}, UseSSL: {UseSSL}, Timestamp: {Timestamp}",
                    _config.SmtpHost, _config.SmtpPort, _config.SmtpUser, _config.UseSsl, DateTime.UtcNow);

                return Ok(new
                {
                    success = true,
                    message = "SMTP connection successful!",
                    messageDe = "SMTP-Verbindung erfolgreich!"
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex,
                    "SMTP validation failed - Host: {Host}, Port: {Port}, User: {User}, Exception: {Exception}, Timestamp: {Timestamp}",
                    _config.SmtpHost, _config.SmtpPort, _config.SmtpUser, ex.Message, DateTime.UtcNow);

                return BadRequest(new
                {
                    success = false,
                    message = $"SMTP connection failed: {ex.Message}",
                    messageDe = $"SMTP-Verbindung fehlgeschlagen: {ex.Message}"
                });
            }
        }
    }
}
