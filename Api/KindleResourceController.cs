using System.Net.Mime;
using System.Reflection;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Net.Http.Headers;

namespace Jellyfin.Plugin.Kindle.Api
{
    [ApiController]
    [Route("KindlePlugin")]
    [AllowAnonymous]
    public class KindleResourceController : ControllerBase
    {
        private const string ResourceName = "Jellyfin.Plugin.Kindle.Web.kindleButton.js";

        /// <summary>
        /// Serves the client script referenced from index.html. Anonymous on purpose:
        /// index.html itself is served before login and the script contains no secrets,
        /// it only calls authenticated endpoints once a session exists.
        /// </summary>
        [HttpGet("ClientScript")]
        public ActionResult GetClientScript()
        {
            var stream = Assembly.GetExecutingAssembly().GetManifestResourceStream(ResourceName);

            if (stream is null)
            {
                return NotFound();
            }

            // The injected tag carries ?v=<plugin version>, so a given URL always maps
            // to the same bytes and can be cached hard. Without this, browsers kept
            // serving the previous version's script after a plugin update.
            Response.Headers[HeaderNames.CacheControl] = "public, max-age=31536000, immutable";

            return File(stream, "application/javascript; charset=utf-8");
        }
    }
}
