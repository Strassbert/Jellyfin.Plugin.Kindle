using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.IO;
using System.Linq;
using System.Net.Mail;
using System.Threading.Tasks;
using Jellyfin.Plugin.Kindle.Configuration;
using Jellyfin.Plugin.Kindle.Models;
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
        private readonly SendHistoryService _historyService;
        private readonly ILogger<KindleController> _logger;

        public KindleController(
            ILibraryManager libraryManager,
            PluginConfiguration config,
            KindleMailService mailService,
            RateLimitingService rateLimiter,
            SendHistoryService historyService,
            ILogger<KindleController> logger)
        {
            _libraryManager = libraryManager;
            _config = config;
            _mailService = mailService;
            _rateLimiter = rateLimiter;
            _historyService = historyService;
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
        /// Get all devices for a user
        /// </summary>
        [HttpGet("Devices")]
        public IActionResult GetUserDevices([FromQuery, Required] string userId)
        {
            if (!_config.EnableMultipleDevices)
            {
                return BadRequest(new { error = "Multiple devices feature is disabled." });
            }

            var devices = _config.UserDevices.TryGetValue(userId, out var userDevices)
                ? userDevices.Where(d => d.IsActive).ToList()
                : new List<UserDevice>();

            return Ok(new { devices });
        }

        /// <summary>
        /// Add a new device for user
        /// </summary>
        [HttpPost("Devices")]
        public IActionResult AddDevice([FromQuery, Required] string userId, [FromBody] AddDeviceRequest request)
        {
            if (!_config.EnableMultipleDevices)
            {
                return BadRequest(new { error = "Multiple devices feature is disabled." });
            }

            if (string.IsNullOrWhiteSpace(request.DeviceName) || string.IsNullOrWhiteSpace(request.Email))
            {
                return BadRequest(new { error = "Device name and email are required." });
            }

            if (!IsValidEmail(request.Email))
            {
                return BadRequest(new { error = "Invalid email format." });
            }

            var devices = _config.UserDevices;
            if (!devices.ContainsKey(userId))
            {
                devices[userId] = new List<UserDevice>();
            }

            // Check for duplicates
            if (devices[userId].Any(d => d.Email.Equals(request.Email, StringComparison.OrdinalIgnoreCase)))
            {
                return BadRequest(new { error = "Email already exists for this user." });
            }

            var newDevice = new UserDevice
            {
                UserId = userId,
                DeviceName = request.DeviceName.Trim(),
                Email = request.Email.Trim(),
                PreferredFormat = request.PreferredFormat ?? "epub",
                IsDefault = devices[userId].Count == 0 // First device is default
            };

            devices[userId].Add(newDevice);
            _config.UserDevices = devices;
            Plugin.Instance.SaveConfiguration();

            _logger.LogInformation(
                "Device added - UserId: {UserId}, DeviceId: {DeviceId}, DeviceName: {DeviceName}, Email: {Email}, Timestamp: {Timestamp}",
                userId, newDevice.Id, newDevice.DeviceName, newDevice.Email, DateTime.UtcNow);

            return Ok(new { message = "Device added successfully.", device = newDevice });
        }

        /// <summary>
        /// Update an existing device
        /// </summary>
        [HttpPut("Devices/{deviceId}")]
        public IActionResult UpdateDevice([FromQuery, Required] string userId, string deviceId, [FromBody] UpdateDeviceRequest request)
        {
            var devices = _config.UserDevices;
            if (!devices.TryGetValue(userId, out var userDevices))
            {
                return NotFound(new { error = "User not found." });
            }

            var device = userDevices.FirstOrDefault(d => d.Id == deviceId);
            if (device == null)
            {
                return NotFound(new { error = "Device not found." });
            }

            if (!string.IsNullOrWhiteSpace(request.DeviceName))
            {
                device.DeviceName = request.DeviceName.Trim();
            }

            if (!string.IsNullOrWhiteSpace(request.Email))
            {
                if (!IsValidEmail(request.Email))
                {
                    return BadRequest(new { error = "Invalid email format." });
                }

                // Check for duplicates (excluding current device)
                if (userDevices.Any(d => d.Id != deviceId && d.Email.Equals(request.Email, StringComparison.OrdinalIgnoreCase)))
                {
                    return BadRequest(new { error = "Email already in use." });
                }

                device.Email = request.Email.Trim();
            }

            if (!string.IsNullOrWhiteSpace(request.PreferredFormat))
            {
                device.PreferredFormat = request.PreferredFormat.ToLower();
            }

            if (request.IsDefault.HasValue && request.IsDefault.Value)
            {
                // Unset other devices as default
                foreach (var d in userDevices.Where(d => d.Id != deviceId))
                {
                    d.IsDefault = false;
                }
                device.IsDefault = true;
            }

            device.ModifiedAt = DateTime.UtcNow;
            _config.UserDevices = devices;
            Plugin.Instance.SaveConfiguration();

            _logger.LogInformation(
                "Device updated - UserId: {UserId}, DeviceId: {DeviceId}, Timestamp: {Timestamp}",
                userId, deviceId, DateTime.UtcNow);

            return Ok(new { message = "Device updated successfully.", device });
        }

        /// <summary>
        /// Delete a device
        /// </summary>
        [HttpDelete("Devices/{deviceId}")]
        public IActionResult DeleteDevice([FromQuery, Required] string userId, string deviceId)
        {
            var devices = _config.UserDevices;
            if (!devices.TryGetValue(userId, out var userDevices))
            {
                return NotFound(new { error = "User not found." });
            }

            var device = userDevices.FirstOrDefault(d => d.Id == deviceId);
            if (device == null)
            {
                return NotFound(new { error = "Device not found." });
            }

            userDevices.Remove(device);

            // If this was default, set another as default
            if (device.IsDefault && userDevices.Count > 0)
            {
                userDevices[0].IsDefault = true;
            }

            _config.UserDevices = devices;
            Plugin.Instance.SaveConfiguration();

            _logger.LogInformation(
                "Device deleted - UserId: {UserId}, DeviceId: {DeviceId}, DeviceName: {DeviceName}, Timestamp: {Timestamp}",
                userId, deviceId, device.DeviceName, DateTime.UtcNow);

            return Ok(new { message = "Device deleted successfully." });
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

        /// <summary>
        /// Get send history for current user
        /// </summary>
        [HttpGet("History")]
        public IActionResult GetUserHistory([FromQuery] int limit = 50)
        {
            if (!_config.EnableSendHistory)
            {
                return BadRequest(new { error = "Send history tracking is disabled." });
            }

            var userId = HttpContext.User.FindFirst("sub")?.Value ??
                        HttpContext.User.FindFirst("http://schemas.xmlsoap.org/ws/2005/05/identity/claims/nameidentifier")?.Value;

            if (string.IsNullOrEmpty(userId))
            {
                return Unauthorized();
            }

            var history = _historyService.GetUserHistory(userId, Math.Min(limit, 100));
            return Ok(new { history });
        }

        /// <summary>
        /// Get user statistics
        /// </summary>
        [HttpGet("Statistics")]
        public IActionResult GetUserStatistics()
        {
            if (!_config.EnableSendHistory)
            {
                return BadRequest(new { error = "Send history tracking is disabled." });
            }

            var userId = HttpContext.User.FindFirst("sub")?.Value ??
                        HttpContext.User.FindFirst("http://schemas.xmlsoap.org/ws/2005/05/identity/claims/nameidentifier")?.Value;

            if (string.IsNullOrEmpty(userId))
            {
                return Unauthorized();
            }

            var stats = _historyService.GetUserStatistics(userId);
            return Ok(new { statistics = stats });
        }

        /// <summary>
        /// Get system-wide statistics (Admin only)
        /// </summary>
        [HttpGet("Statistics/System")]
        [Authorize(Policy = "RequireAdministratorRole")]
        public IActionResult GetSystemStatistics()
        {
            if (!_config.EnableSendHistory)
            {
                return BadRequest(new { error = "Send history tracking is disabled." });
            }

            var stats = _historyService.GetSystemStatistics();
            return Ok(new { statistics = stats });
        }

        /// <summary>
        /// Clear send history for a user (Admin only)
        /// </summary>
        [HttpDelete("History")]
        [Authorize(Policy = "RequireAdministratorRole")]
        public IActionResult ClearHistory([FromQuery] string userId)
        {
            if (string.IsNullOrWhiteSpace(userId))
            {
                return BadRequest(new { error = "UserId is required." });
            }

            _historyService.ClearUserHistory(userId);
            return Ok(new { message = "User history cleared." });
        }

        /// <summary>
        /// Clear all send history (Admin only)
        /// </summary>
        [HttpDelete("History/All")]
        [Authorize(Policy = "RequireAdministratorRole")]
        public IActionResult ClearAllHistory()
        {
            _historyService.ClearAllHistory();
            return Ok(new { message = "All history cleared." });
        }
    }

    /// <summary>
    /// Request model for adding a new device
    /// </summary>
    public class AddDeviceRequest
    {
        public string DeviceName { get; set; }
        public string Email { get; set; }
        public string PreferredFormat { get; set; } = "epub";
    }

    /// <summary>
    /// Request model for updating a device
    /// </summary>
    public class UpdateDeviceRequest
    {
        public string DeviceName { get; set; }
        public string Email { get; set; }
        public string PreferredFormat { get; set; }
        public bool? IsDefault { get; set; }
    }
}
