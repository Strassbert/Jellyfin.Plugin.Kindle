using System;
using System.Linq;
using System.Threading.Tasks;
using Jellyfin.Plugin.Kindle.Services;
using MediaBrowser.Controller.Library;
using MediaBrowser.Controller.Net;
using MediaBrowser.Model.IO;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;

namespace Jellyfin.Plugin.Kindle.Api
{
    [ApiController]
    [Route("Kindle")]
    public class KindleController : ControllerBase
    {
        private readonly ILibraryManager _libraryManager;
        private readonly KindleMailService _mailService;
        private readonly IFileSystem _fileSystem;

        public KindleController(ILibraryManager libraryManager, KindleMailService mailService, IFileSystem fileSystem)
        {
            _libraryManager = libraryManager;
            _mailService = mailService;
            _fileSystem = fileSystem;
        }

        [HttpPost("Send")]
        public async Task<IActionResult> SendToKindle([FromQuery] string itemId, [FromQuery] string userId)
        {
            var item = _libraryManager.GetItemById(itemId);
            var config = Plugin.Instance.Configuration;

            if (item == null) return NotFound("Buch nicht gefunden.");
            
            // 1. Format-Prüfung (Regel 4.1 & 5)
            var extension = Path.GetExtension(item.Path);
            if (!KindleFormatValidator.IsCompatible(extension))
            {
                return BadRequest("Format wird vom Kindle nicht unterstützt.");
            }

            // 2. User-Email checken
            if (!config.UserKindleEmails.TryGetValue(userId, out var kindleEmail) || string.IsNullOrEmpty(kindleEmail))
            {
                return BadRequest("Keine Kindle-Email hinterlegt.");
            }

            // 3. Versand starten
            try 
            {
                // Hier könnten wir später den MailQueueService für echten asynchronen Versand einbauen
                await _mailService.SendBookAsync(kindleEmail, item.Path, item.Name + extension, config);
                return Ok();
            }
            catch (Exception ex)
            {
                return StatusCode(500, ex.Message);
            }
        }
    }
}