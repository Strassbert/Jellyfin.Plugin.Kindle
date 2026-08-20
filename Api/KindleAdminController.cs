using System;
using System.ComponentModel.DataAnnotations;
using System.Threading;
using System.Threading.Tasks;
using Jellyfin.Plugin.Kindle.Configuration;
using Jellyfin.Plugin.Kindle.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;

namespace Jellyfin.Plugin.Kindle.Api
{
    /// <summary>
    /// Administrator-only settings API.
    /// </summary>
    /// <remarks>
    /// The configuration page uses these endpoints instead of Jellyfin's generic
    /// /Plugins/{id}/Configuration, which serialises the whole configuration class and
    /// therefore hands the SMTP password to the browser on every page load. BasePlugin
    /// exposes Configuration as a non-virtual property, so the generic endpoint cannot
    /// be filtered; keeping the page off it at least stops the password from reaching
    /// the DOM, browser password managers and screenshots. An administrator token can
    /// still read it from the generic endpoint - that is a Jellyfin limitation, not
    /// something this plugin can close.
    /// </remarks>
    [ApiController]
    [Route("Kindle/Admin")]
    [Authorize(Policy = "RequiresElevation")]
    public class KindleAdminController : ControllerBase
    {
        private readonly KindleMailService _mailService;
        private readonly ILogger<KindleAdminController> _logger;

        public KindleAdminController(KindleMailService mailService, ILogger<KindleAdminController> logger)
        {
            _mailService = mailService;
            _logger = logger;
        }

        [HttpGet("Config")]
        public ActionResult<AdminConfigDto> GetConfig()
        {
            var config = Plugin.Instance?.Configuration;
            if (config is null)
            {
                return StatusCode(503, KindleController.ErrorPayload("Plugin is not initialized.", "Plugin ist nicht initialisiert."));
            }

            return Ok(new AdminConfigDto
            {
                SmtpHost = config.SmtpHost,
                SmtpPort = config.SmtpPort,
                Security = config.Security,
                SmtpUser = config.SmtpUser,
                HasPassword = !string.IsNullOrEmpty(config.SmtpPassword),
                SenderEmail = config.SenderEmail,
                SenderName = config.SenderName,
                RequestConversion = config.RequestConversion,
                MaxMessageSizeMb = config.MaxMessageSizeMb,
                ConfiguredUserCount = config.UserKindleEmails.Count,
                MaxFileSizeMb = Math.Round(
                    KindleFormatValidator.MaxFileSizeBytes(config.MaxMessageSizeMb) / (1024.0 * 1024.0), 1)
            });
        }

        [HttpPost("Config")]
        public ActionResult SaveConfig([FromBody] AdminConfigDto update)
        {
            var config = Plugin.Instance?.Configuration;
            if (config is null)
            {
                return StatusCode(503, KindleController.ErrorPayload("Plugin is not initialized.", "Plugin ist nicht initialisiert."));
            }

            if (update.SmtpPort is <= 0 or > 65535)
            {
                return BadRequest(KindleController.ErrorPayload("SMTP port is out of range.", "SMTP-Port liegt außerhalb des gültigen Bereichs."));
            }

            config.SmtpHost = update.SmtpHost?.Trim() ?? string.Empty;
            config.SmtpPort = update.SmtpPort;
            config.Security = Enum.IsDefined(typeof(SmtpSecurity), update.Security) ? update.Security : (int)SmtpSecurity.Auto;
            config.SmtpUser = update.SmtpUser?.Trim() ?? string.Empty;
            config.SenderEmail = update.SenderEmail?.Trim() ?? string.Empty;
            config.SenderName = string.IsNullOrWhiteSpace(update.SenderName) ? "Jellyfin" : update.SenderName.Trim();
            config.RequestConversion = update.RequestConversion;
            config.MaxMessageSizeMb = update.MaxMessageSizeMb is > 0 and <= 200 ? update.MaxMessageSizeMb : 50;

            // The page never receives the stored password, so an empty field means
            // "unchanged" rather than "clear it". Clearing needs an explicit flag,
            // which the relay-without-login case actually requires.
            if (update.ClearPassword)
            {
                config.SmtpPassword = string.Empty;
            }
            else if (!string.IsNullOrEmpty(update.SmtpPassword))
            {
                config.SmtpPassword = update.SmtpPassword;
            }

            config.ConfigVersion = PluginConfiguration.LatestConfigVersion;
            config.UseSsl = config.SecurityMode != SmtpSecurity.None;

            Plugin.Instance!.SaveConfiguration();
            _logger.LogInformation("[E-Book Share] Configuration updated by an administrator.");

            return Ok(new { message = "Settings saved.", messageDe = "Einstellungen gespeichert." });
        }

        [HttpPost("TestConnection")]
        public async Task<IActionResult> TestConnection(CancellationToken cancellationToken)
        {
            var config = Plugin.Instance?.Configuration;
            if (config is null)
            {
                return StatusCode(503, KindleController.ErrorPayload("Plugin is not initialized.", "Plugin ist nicht initialisiert."));
            }

            var result = await _mailService.TestConnectionAsync(config, cancellationToken).ConfigureAwait(false);

            return result.Success
                ? Ok(new { message = "Connection successful.", messageDe = "Verbindung erfolgreich." })
                : StatusCode(502, KindleController.DescribeMailFailure(result));
        }

        [HttpPost("TestMail")]
        public async Task<IActionResult> TestMail([FromQuery, Required] string address, CancellationToken cancellationToken)
        {
            var config = Plugin.Instance?.Configuration;
            if (config is null)
            {
                return StatusCode(503, KindleController.ErrorPayload("Plugin is not initialized.", "Plugin ist nicht initialisiert."));
            }

            var recipient = address?.Trim() ?? string.Empty;
            if (!Validation.IsPlausibleEmailAddress(recipient))
            {
                return BadRequest(KindleController.ErrorPayload("Invalid email format.", "Ungültiges E-Mail-Format."));
            }

            var result = await _mailService.SendTestAsync(recipient, config, cancellationToken).ConfigureAwait(false);

            return result.Success
                ? Ok(new { message = "Test message sent.", messageDe = "Testnachricht versendet." })
                : StatusCode(502, KindleController.DescribeMailFailure(result));
        }

        public class AdminConfigDto
        {
            public string SmtpHost { get; set; } = string.Empty;

            public int SmtpPort { get; set; } = 587;

            public int Security { get; set; }

            public string SmtpUser { get; set; } = string.Empty;

            /// <summary>Read-only indicator; the password itself is never sent out.</summary>
            public bool HasPassword { get; set; }

            /// <summary>Write-only. Empty means "keep the stored password".</summary>
            public string? SmtpPassword { get; set; }

            /// <summary>Write-only. Explicitly removes the stored password.</summary>
            public bool ClearPassword { get; set; }

            public string SenderEmail { get; set; } = string.Empty;

            public string SenderName { get; set; } = "Jellyfin";

            public bool RequestConversion { get; set; }

            public int MaxMessageSizeMb { get; set; } = 50;

            public int ConfiguredUserCount { get; set; }

            public double MaxFileSizeMb { get; set; }
        }
    }
}
