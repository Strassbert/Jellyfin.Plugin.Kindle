using System.Reflection;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Jellyfin.Plugin.Kindle.Api
{
    [ApiController]
    [Route("KindlePlugin")]
    [AllowAnonymous]
    public class KindleResourceController : ControllerBase
    {
        [HttpGet("ClientScript")]
        [Produces("application/javascript")]
        public ActionResult GetClientScript()
        {
            var stream = Assembly.GetExecutingAssembly()
                .GetManifestResourceStream("Jellyfin.Plugin.Kindle.Web.kindleButton.js");

            if (stream == null)
            {
                return NotFound();
            }

            return File(stream, "application/javascript");
        }
    }
}
