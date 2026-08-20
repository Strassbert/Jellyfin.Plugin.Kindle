using System;
using System.ComponentModel.DataAnnotations;
using System.IO;
using System.Threading;
using System.Threading.Tasks;
using Jellyfin.Plugin.Kindle.Configuration;
using Jellyfin.Plugin.Kindle.Services;
using Jellyfin.Database.Implementations.Entities;
using MediaBrowser.Controller.Library;
using MediaBrowser.Controller.Net;
using MediaBrowser.Model.Activity;
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
        private readonly IActivityManager _activityManager;
        private readonly KindleMailService _mailService;
        private readonly ILogger<KindleController> _logger;

        public KindleController(
            ILibraryManager libraryManager,
            IUserManager userManager,
            IAuthorizationContext authContext,
            IActivityManager activityManager,
            KindleMailService mailService,
            ILogger<KindleController> logger)
        {
            _libraryManager = libraryManager;
            _userManager = userManager;
            _authContext = authContext;
            _activityManager = activityManager;
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
                return StatusCode(503, ErrorPayload("Plugin is not initialized.", "Plugin ist nicht initialisiert."));
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
                return StatusCode(503, ErrorPayload("Plugin is not initialized.", "Plugin ist nicht initialisiert."));
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
                return NotFound(ErrorPayload("Item not found.", "Buch nicht gefunden."));
            }

            var extension = Path.GetExtension(item.Path);
            if (!KindleFormatValidator.IsCompatible(extension))
            {
                return BadRequest(ErrorPayload(
                    $"Format '{extension}' is not supported by your reader.",
                    $"Format '{extension}' wird vom E-Book Reader nicht unterstützt.",
                    "FORMAT"));
            }

            if (string.IsNullOrEmpty(item.Path) || !System.IO.File.Exists(item.Path))
            {
                return NotFound(ErrorPayload("File not found on disk.", "Datei nicht auf der Festplatte gefunden."));
            }

            var fileInfo = new FileInfo(item.Path);
            var maxBytes = KindleFormatValidator.MaxFileSizeBytes(config.MaxMessageSizeMb);
            if (fileInfo.Length > maxBytes)
            {
                var sizeMb = fileInfo.Length / (1024.0 * 1024.0);
                var maxMb = maxBytes / (1024.0 * 1024.0);
                return BadRequest(ErrorPayload(
                    $"File is too large ({sizeMb:F1} MB). The limit is {maxMb:F1} MB.",
                    $"Datei ist zu groß ({sizeMb:F1} MB). Das Limit liegt bei {maxMb:F1} MB.",
                    "TOO_LARGE"));
            }

            var kindleEmail = config.GetUserEmail(userId.ToString("N"));
            if (kindleEmail is null)
            {
                return BadRequest(ErrorPayload(
                    "No reader email configured. Please set it in your user settings.",
                    "Keine E-Book Reader E-Mail hinterlegt. Bitte in den Benutzereinstellungen konfigurieren.",
                    "NO_KINDLE_EMAIL"));
            }

            var attachmentName = Validation.BuildAttachmentName(item.Name, extension);

            await SendGate.WaitAsync(cancellationToken).ConfigureAwait(false);
            try
            {
                var result = await _mailService
                    .SendBookAsync(kindleEmail, item.Path, attachmentName, config, cancellationToken)
                    .ConfigureAwait(false);

                await LogActivityAsync(userId, item.Name, result).ConfigureAwait(false);

                if (result.Success)
                {
                    _logger.LogInformation("[E-Book Share] '{Name}' sent for user {UserId}.", item.Name, userId);
                    return Ok(new { message = "Sent to your reader.", messageDe = "An E-Book Reader gesendet." });
                }

                return StatusCode(502, DescribeMailFailure(result));
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
                return StatusCode(503, ErrorPayload("Plugin is not initialized.", "Plugin ist nicht initialisiert."));
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
                return StatusCode(503, ErrorPayload("Plugin is not initialized.", "Plugin ist nicht initialisiert."));
            }

            var userId = await GetCallerUserIdAsync().ConfigureAwait(false);
            if (userId == Guid.Empty)
            {
                return Forbid();
            }

            var trimmed = email?.Trim() ?? string.Empty;
            if (!Validation.IsPlausibleEmailAddress(trimmed))
            {
                return BadRequest(ErrorPayload("Invalid email format.", "Ungültiges E-Mail-Format."));
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
                return StatusCode(503, ErrorPayload("Plugin is not initialized.", "Plugin ist nicht initialisiert."));
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
        /// Sends a short test message to the caller's own stored address.
        /// </summary>
        /// <remarks>
        /// The connection test on the admin page cannot prove delivery: Amazon accepts
        /// the SMTP session and then discards mail whose sender is not on the account's
        /// approved list, with no bounce. Letting the user send a tiny message makes
        /// that failure visible without pushing a 30 MB book through the provider.
        /// </remarks>
        [HttpPost("TestMail")]
        public async Task<IActionResult> SendTestMail(CancellationToken cancellationToken)
        {
            var config = GetConfiguration();
            if (config is null)
            {
                return StatusCode(503, ErrorPayload("Plugin is not initialized.", "Plugin ist nicht initialisiert."));
            }

            var userId = await GetCallerUserIdAsync().ConfigureAwait(false);
            if (userId == Guid.Empty)
            {
                return Forbid();
            }

            var recipient = config.GetUserEmail(userId.ToString("N"));
            if (recipient is null)
            {
                return BadRequest(ErrorPayload(
                    "Save a reader address first.",
                    "Bitte zuerst eine Reader-Adresse speichern.",
                    "NO_KINDLE_EMAIL"));
            }

            await SendGate.WaitAsync(cancellationToken).ConfigureAwait(false);
            try
            {
                var result = await _mailService.SendTestAsync(recipient, config, cancellationToken).ConfigureAwait(false);

                return result.Success
                    ? Ok(new { message = "Test message sent.", messageDe = "Testnachricht versendet." })
                    : StatusCode(502, DescribeMailFailure(result));
            }
            finally
            {
                SendGate.Release();
            }
        }

        /// <summary>
        /// Mirrors sends into the dashboard activity feed. Failures here are swallowed:
        /// a book that was delivered must not be reported as failed because the
        /// bookkeeping entry could not be written.
        /// </summary>
        private async Task LogActivityAsync(Guid userId, string? itemName, MailResult result)
        {
            try
            {
                var user = _userManager.GetUserById(userId);
                var userName = user?.Username ?? userId.ToString("N");
                var title = itemName ?? "a book";

                var entry = new ActivityLog(
                    result.Success
                        ? $"{userName} sent \"{title}\" to their e-book reader"
                        : $"Sending \"{title}\" to {userName}'s e-book reader failed",
                    "EBookShare",
                    userId)
                {
                    ShortOverview = result.Success ? null : result.Detail,
                    LogSeverity = result.Success ? LogLevel.Information : LogLevel.Error
                };

                await _activityManager.CreateAsync(entry).ConfigureAwait(false);
            }
            catch (Exception ex)
            {
                _logger.LogDebug(ex, "[E-Book Share] Could not write the activity log entry.");
            }
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

        internal static object ErrorPayload(string en, string de, string? code = null) =>
            new { error = en, errorDe = de, code };

        internal static object DescribeMailFailure(MailResult result) => result.Failure switch
        {
            MailFailure.NotConfigured => ErrorPayload(
                "SMTP is not configured. Ask your administrator to fill in the plugin settings.",
                "SMTP ist nicht konfiguriert. Bitte den Administrator, die Plugin-Einstellungen auszufüllen.",
                "SMTP_NOT_CONFIGURED"),
            MailFailure.Authentication => ErrorPayload(
                "SMTP login failed. Check username and password (an app password is usually required).",
                "SMTP-Anmeldung fehlgeschlagen. Benutzername und Passwort prüfen (meist wird ein App-Passwort benötigt).",
                "SMTP_AUTH"),
            MailFailure.Connection => ErrorPayload(
                "Could not reach the SMTP server. Check host, port and encryption mode.",
                "SMTP-Server nicht erreichbar. Host, Port und Verschlüsselungsmodus prüfen.",
                "SMTP_CONNECTION"),
            MailFailure.Timeout => ErrorPayload(
                "The SMTP server did not respond in time.",
                "Der SMTP-Server hat nicht rechtzeitig geantwortet.",
                "SMTP_TIMEOUT"),
            MailFailure.Rejected => ErrorPayload(
                $"The mail server rejected the message: {result.Detail}",
                $"Der Mailserver hat die Nachricht abgelehnt: {result.Detail}",
                "SMTP_REJECTED"),
            _ => ErrorPayload(
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
