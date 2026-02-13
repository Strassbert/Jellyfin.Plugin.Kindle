using System.Reflection;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;

namespace Jellyfin.Plugin.Kindle.Api
{
    /// <summary>
    /// Liefert die JavaScript-Datei über eine saubere URL aus: /KindlePlugin/ClientScript
    /// </summary>
    [ApiController]
    [Route("KindlePlugin")]
    public class KindleResourceController : ControllerBase
    {
        private readonly ILogger<KindleResourceController> _logger;

        public KindleResourceController(ILogger<KindleResourceController> logger)
        {
            _logger = logger;
        }

        [HttpGet("ClientScript")]
        [Produces("application/javascript")]
        public ActionResult GetClientScript()
        {
            var assembly = Assembly.GetExecutingAssembly();
            // ACHTUNG: Der Pfad muss exakt stimmen (Namespace + Ordner + Dateiname)
            var resourceName = "Jellyfin.Plugin.Kindle.Web.kindleButton.js";

            var stream = assembly.GetManifestResourceStream(resourceName);
            if (stream == null)
            {
                _logger.LogError("Kindle Plugin: Konnte eingebettete Ressource {Resource} nicht finden.", resourceName);
                return NotFound();
            }

            return File(stream, "application/javascript");
        }
    }
}