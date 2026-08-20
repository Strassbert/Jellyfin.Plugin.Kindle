using System;
using System.ComponentModel.DataAnnotations;
using System.IO;
using System.Threading;
using System.Threading.Tasks;
using Jellyfin.Plugin.Kindle.Configuration;
using Jellyfin.Plugin.Kindle.Services;
using MediaBrowser.Controller.Library;
using MediaBrowser.Controller.Net;
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
        // Two concurrent SMTP sessions is plenty for a household and keeps a burst of
        // clicks from opening a connection per request against the provider.
        private static readonly SemaphoreSlim SendGate = new(2, 2);

        private readonly ILibraryManager _libraryManager;
        private readonly IUserManager _userManager;
        private readonly IAuthorizationContext _authContext;
        private readonly KindleMailService _mailService;
        private readonly ILogger<KindleController> _logger;

        public KindleController(
            ILibraryManager libraryManager,
            IUserManager userManager,
            IAuthorizationContext authContext,
            KindleMailService mailService,
            ILogger<KindleController> logger)
        {
            _libraryManager = libraryManager;
            _userManager = userManager;
            _authContext = authContext;
            _mailService = mailService;
            _logger = logger;
        }

        /// <summary>
        /// Reports whether an item can be sent, so the client can disable the button
        /// with a reason instead of failing only after the user clicks it.
        /// </summary>
        [HttpGet("Status")]
        public async Task<IActionResult> GetStatus([FromQuery] string? itemId)
        {
            var config = GetConfiguration();
            if (config is null)
            {
                return StatusCode(503, Error("Plugin is not initialized.", "Plugin ist nicht initialisiert."));
            }

            var userId = await GetCallerUserIdAsync().ConfigureAwait(false);
            if (userId == Guid.Empty)
            {
                return Forbid();
            }

            var response = new StatusResponse
            {
                SmtpConfigured = !string.IsNullOrWhiteSpace(config.SmtpHost),
                HasEmail = config.GetUserEmail(userId.ToString("N")) is not null,
                // Shown on the user settings page: the reader vendor only accepts mail
                // from approved senders, so the user needs to know which address to approve.
                SenderEmail = string.IsNullOrWhiteSpace(config.SenderEmail) ? config.SmtpUser : config.SenderEmail,
                MaxFileSizeMb = Math.Round(KindleFormatValidator.MaxFileSizeBytes(config.MaxMessageSizeMb) / (1024.0 * 1024.0), 1)
            };

            if (string.IsNullOrWhiteSpace(itemId))
            {
                return Ok(response);
            }

            var item = _libraryManager.GetItemById(itemId);
            if (item is null || !IsVisibleTo(item, userId))
            {
                response.Sendable = false;
                response.Reason = "NOT_FOUND";
                return Ok(response);
            }

            var extension = Path.GetExtension(item.Path);
            response.Extension = extension;

            if (!KindleFormatValidator.IsCompatible(extension))
            {
                response.Sendable = false;
                response.Reason = "FORMAT";
                return Ok(response);
            }

            if (string.IsNullOrEmpty(item.Path) || !System.IO.File.Exists(item.Path))
            {
                response.Sendable = false;
                response.Reason = "NOT_FOUND";
                return Ok(response);
            }

            var length = new FileInfo(item.Path).Length;
            response.FileSizeMb = Math.Round(length / (1024.0 * 1024.0), 1);

            if (length > KindleFormatValidator.MaxFileSizeBytes(config.MaxMessageSizeMb))
            {
                response.Sendable = false;
                response.Reason = "TOO_LARGE";
                return Ok(response);
            }

            response.Sendable = true;
            return Ok(response);
        }

        [HttpPost("Send")]
        public async Task<IActionResult> SendToKindle(
            [FromQuery, Required] string itemId,
            CancellationToken cancellationToken)
        {
            var config = GetConfiguration();
            if (config is null)
            {
                return StatusCode(503, Error("Plugin is not initialized.", "Plugin ist nicht initialisiert."));
            }

            // The caller is taken from the authenticated session, never from the
            // request. Trusting a userId query parameter let any signed-in user send
            // books as - and read the address of - anybody else.
            var userId = await GetCallerUserIdAsync().ConfigureAwait(false);
            if (userId == Guid.Empty)
            {
                return Forbid();
            }

            var item = _libraryManager.GetItemById(itemId);
            if (item is null || !IsVisibleTo(item, userId))
            {
                return NotFound(Error("Item not found.", "Buch nicht gefunden."));
            }

            var extension = Path.GetExtension(item.Path);
            if (!KindleFormatValidator.IsCompatible(extension))
            {
                return BadRequest(Error(
                    $"Format '{extension}' is not supported by your reader.",
                    $"Format '{extension}' wird vom E-Book Reader nicht unterstützt.",
                    "FORMAT"));
            }

            if (string.IsNullOrEmpty(item.Path) || !System.IO.File.Exists(item.Path))
            {
                return NotFound(Error("File not found on disk.", "Datei nicht auf der Festplatte gefunden."));
            }

            var fileInfo = new FileInfo(item.Path);
            var maxBytes = KindleFormatValidator.MaxFileSizeBytes(config.MaxMessageSizeMb);
            if (fileInfo.Length > maxBytes)
            {
                var sizeMb = fileInfo.Length / (1024.0 * 1024.0);
                var maxMb = maxBytes / (1024.0 * 1024.0);
                return BadRequest(Error(
                    $"File is too large ({sizeMb:F1} MB). The limit is {maxMb:F1} MB.",
                    $"Datei ist zu groß ({sizeMb:F1} MB). Das Limit liegt bei {maxMb:F1} MB.",
                    "TOO_LARGE"));
            }

            var kindleEmail = config.GetUserEmail(userId.ToString("N"));
            if (kindleEmail is null)
            {
                return BadRequest(Error(
                    "No reader email configured. Please set it in your user settings.",
                    "Keine E-Book Reader E-Mail hinterlegt. Bitte in den Benutzereinstellungen konfigurieren.",
                    "NO_KINDLE_EMAIL"));
            }

            var attachmentName = BuildAttachmentName(item.Name, extension);

            await SendGate.WaitAsync(cancellationToken).ConfigureAwait(false);
            try
            {
                var result = await _mailService
                    .SendBookAsync(kindleEmail, item.Path, attachmentName, config, cancellationToken)
                    .ConfigureAwait(false);

                if (result.Success)
                {
                    _logger.LogInformation("[E-Book Share] '{Name}' sent for user {UserId}.", item.Name, userId);
                    return Ok(new { message = "Sent to your reader.", messageDe = "An E-Book Reader gesendet." });
                }

                return StatusCode(502, DescribeFailure(result));
            }
            finally
            {
                SendGate.Release();
            }
        }

        [HttpGet("UserEmail")]
        public async Task<IActionResult> GetUserEmail()
        {
            var config = GetConfiguration();
            if (config is null)
            {
                return StatusCode(503, Error("Plugin is not initialized.", "Plugin ist nicht initialisiert."));
            }

            var userId = await GetCallerUserIdAsync().ConfigureAwait(false);
            if (userId == Guid.Empty)
            {
                return Forbid();
            }

            return Ok(new { email = config.GetUserEmail(userId.ToString("N")) ?? string.Empty });
        }

        [HttpPost("UserEmail")]
        public async Task<IActionResult> SetUserEmail([FromQuery, Required] string email)
        {
            var config = GetConfiguration();
            if (config is null)
            {
                return StatusCode(503, Error("Plugin is not initialized.", "Plugin ist nicht initialisiert."));
            }

            var userId = await GetCallerUserIdAsync().ConfigureAwait(false);
            if (userId == Guid.Empty)
            {
                return Forbid();
            }

            var trimmed = email?.Trim() ?? string.Empty;
            if (!IsPlausibleEmail(trimmed))
            {
                return BadRequest(Error("Invalid email format.", "Ungültiges E-Mail-Format."));
            }

            config.SetUserEmail(userId.ToString("N"), trimmed);
            Plugin.Instance!.SaveConfiguration();

            _logger.LogInformation("[E-Book Share] Reader email updated for user {UserId}.", userId);
            return Ok(new { message = "Reader email saved.", messageDe = "E-Book Reader-E-Mail gespeichert." });
        }

        [HttpDelete("UserEmail")]
        public async Task<IActionResult> DeleteUserEmail()
        {
            var config = GetConfiguration();
            if (config is null)
            {
                return StatusCode(503, Error("Plugin is not initialized.", "Plugin ist nicht initialisiert."));
            }

            var userId = await GetCallerUserIdAsync().ConfigureAwait(false);
            if (userId == Guid.Empty)
            {
                return Forbid();
            }

            config.SetUserEmail(userId.ToString("N"), null);
            Plugin.Instance!.SaveConfiguration();

            _logger.LogInformation("[E-Book Share] Reader email removed for user {UserId}.", userId);
            return Ok(new { message = "Reader email removed.", messageDe = "E-Book Reader E-Mail entfernt." });
        }

        /// <summary>
        /// Verifies the stored SMTP settings from the admin page without sending a book.
        /// </summary>
        [HttpPost("TestConnection")]
        [Authorize(Policy = "RequiresElevation")]
        public async Task<IActionResult> TestConnection(CancellationToken cancellationToken)
        {
            var config = GetConfiguration();
            if (config is null)
            {
                return StatusCode(503, Error("Plugin is not initialized.", "Plugin ist nicht initialisiert."));
            }

            var result = await _mailService.TestConnectionAsync(config, cancellationToken).ConfigureAwait(false);

            return result.Success
                ? Ok(new { message = "Connection successful.", messageDe = "Verbindung erfolgreich." })
                : StatusCode(502, DescribeFailure(result));
        }

        private PluginConfiguration? GetConfiguration() => Plugin.Instance?.Configuration;

        private async Task<Guid> GetCallerUserIdAsync()
        {
            var auth = await _authContext.GetAuthorizationInfo(Request.HttpContext).ConfigureAwait(false);
            return auth.UserId;
        }

        private bool IsVisibleTo(MediaBrowser.Controller.Entities.BaseItem item, Guid userId)
        {
            var user = _userManager.GetUserById(userId);
            if (user is null)
            {
                return false;
            }

            // Without this a user could send any item in the server by guessing its id,
            // including items in libraries they have no access to.
            return item.IsVisible(user);
        }

        /// <summary>
        /// Uses the library item's name for the attachment so the reader shows the
        /// book title instead of the on-disk filename, while keeping the file system
        /// characters that would break a MIME filename out of it.
        /// </summary>
        private static string BuildAttachmentName(string? itemName, string extension)
        {
            var name = string.IsNullOrWhiteSpace(itemName) ? "book" : itemName.Trim();

            foreach (var invalid in Path.GetInvalidFileNameChars())
            {
                name = name.Replace(invalid, '_');
            }

            name = name.Replace('"', '_').Replace('\\', '_').Replace('/', '_');

            if (name.Length > 100)
            {
                name = name[..100].TrimEnd();
            }

            return name + extension;
        }

        private static bool IsPlausibleEmail(string email)
        {
            if (string.IsNullOrWhiteSpace(email) || email.Length > 254)
            {
                return false;
            }

            var at = email.IndexOf('@', StringComparison.Ordinal);
            if (at <= 0 || at != email.LastIndexOf('@'))
            {
                return false;
            }

            var domain = email[(at + 1)..];
            return domain.Length >= 3
                   && domain.Contains('.', StringComparison.Ordinal)
                   && !domain.StartsWith('.')
                   && !domain.EndsWith('.')
                   && !email.Contains(' ', StringComparison.Ordinal);
        }

        private static object Error(string en, string de, string? code = null) =>
            new { error = en, errorDe = de, code };

        private static object DescribeFailure(MailResult result) => result.Failure switch
        {
            MailFailure.NotConfigured => Error(
                "SMTP is not configured. Ask your administrator to fill in the plugin settings.",
                "SMTP ist nicht konfiguriert. Bitte den Administrator, die Plugin-Einstellungen auszufüllen.",
                "SMTP_NOT_CONFIGURED"),
            MailFailure.Authentication => Error(
                "SMTP login failed. Check username and password (an app password is usually required).",
                "SMTP-Anmeldung fehlgeschlagen. Benutzername und Passwort prüfen (meist wird ein App-Passwort benötigt).",
                "SMTP_AUTH"),
            MailFailure.Connection => Error(
                "Could not reach the SMTP server. Check host, port and encryption mode.",
                "SMTP-Server nicht erreichbar. Host, Port und Verschlüsselungsmodus prüfen.",
                "SMTP_CONNECTION"),
            MailFailure.Timeout => Error(
                "The SMTP server did not respond in time.",
                "Der SMTP-Server hat nicht rechtzeitig geantwortet.",
                "SMTP_TIMEOUT"),
            MailFailure.Rejected => Error(
                $"The mail server rejected the message: {result.Detail}",
                $"Der Mailserver hat die Nachricht abgelehnt: {result.Detail}",
                "SMTP_REJECTED"),
            _ => Error(
                "Failed to send email. Please check the SMTP settings.",
                "E-Mail-Versand fehlgeschlagen. Bitte SMTP-Einstellungen prüfen.",
                "SMTP_UNKNOWN")
        };

        private sealed class StatusResponse
        {
            public bool SmtpConfigured { get; set; }

            public bool HasEmail { get; set; }

            public string? SenderEmail { get; set; }

            public bool Sendable { get; set; }

            public string? Reason { get; set; }

            public string? Extension { get; set; }

            public double FileSizeMb { get; set; }

            public double MaxFileSizeMb { get; set; }
        }
    }
}
