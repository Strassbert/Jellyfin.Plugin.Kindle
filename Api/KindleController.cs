using System;
using System.ComponentModel.DataAnnotations;
using System.IO;
using System.Threading.Tasks;
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
        private readonly KindleMailService _mailService;
        private readonly ILogger<KindleController> _logger;

        public KindleController(
            ILibraryManager libraryManager,
            KindleMailService mailService,
            ILogger<KindleController> logger)
        {
            _libraryManager = libraryManager;
            _mailService = mailService;
            _logger = logger;
        }

        [HttpPost("Send")]
        public async Task<IActionResult> SendToKindle(
            [FromQuery, Required] string itemId,
            [FromQuery, Required] string userId)
        {
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
                    error = $"Format '{extension}' is not supported by Kindle.",
                    errorDe = $"Format '{extension}' wird vom Kindle nicht unterstützt."
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
                    error = $"File is too large ({sizeMb:F1} MB). Amazon Kindle limit is 50 MB.",
                    errorDe = $"Datei ist zu groß ({sizeMb:F1} MB). Amazon Kindle Limit ist 50 MB."
                });
            }

            // User email check
            var config = Plugin.Instance.Configuration;
            if (!config.UserKindleEmails.TryGetValue(userId, out var kindleEmail) || string.IsNullOrEmpty(kindleEmail))
            {
                return BadRequest(new
                {
                    error = "No Kindle email configured. Please set your Kindle email in user settings.",
                    errorDe = "Keine Kindle-E-Mail hinterlegt. Bitte in den Benutzereinstellungen konfigurieren.",
                    code = "NO_KINDLE_EMAIL"
                });
            }

            try
            {
                await _mailService.SendBookAsync(kindleEmail, item.Path, item.Name + extension, config);
                _logger.LogInformation("Book '{Name}' sent to {Email} for user {UserId}.", item.Name, kindleEmail, userId);
                return Ok(new { message = "Sent to Kindle.", messageDe = "An Kindle gesendet." });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to send book '{Name}' to Kindle for user {UserId}.", item.Name, userId);
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
            var config = Plugin.Instance.Configuration;
            config.UserKindleEmails.TryGetValue(userId, out var email);
            return Ok(new { email = email ?? string.Empty });
        }

        [HttpPost("UserEmail")]
        public IActionResult SetUserEmail([FromQuery, Required] string userId, [FromQuery, Required] string email)
        {
            if (string.IsNullOrWhiteSpace(email))
            {
                return BadRequest(new { error = "Email cannot be empty.", errorDe = "E-Mail darf nicht leer sein." });
            }

            // Basic email format validation
            if (!email.Contains('@') || !email.Contains('.'))
            {
                return BadRequest(new { error = "Invalid email format.", errorDe = "Ungültiges E-Mail-Format." });
            }

            var config = Plugin.Instance.Configuration;
            config.UserKindleEmails[userId] = email.Trim();
            Plugin.Instance.SaveConfiguration();

            _logger.LogInformation("Kindle email updated for user {UserId}.", userId);
            return Ok(new { message = "Kindle email saved.", messageDe = "Kindle-E-Mail gespeichert." });
        }

        [HttpDelete("UserEmail")]
        public IActionResult DeleteUserEmail([FromQuery, Required] string userId)
        {
            var config = Plugin.Instance.Configuration;
            config.UserKindleEmails.Remove(userId);
            Plugin.Instance.SaveConfiguration();

            _logger.LogInformation("Kindle email removed for user {UserId}.", userId);
            return Ok(new { message = "Kindle email removed.", messageDe = "Kindle-E-Mail entfernt." });
        }
    }
}
