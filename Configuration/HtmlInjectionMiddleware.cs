using System.IO;
using System.Text;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;

namespace Jellyfin.Plugin.Kindle.Configuration
{
    public class HtmlInjectionMiddleware
    {
        private readonly RequestDelegate _next;
        private readonly ILogger<HtmlInjectionMiddleware> _logger;

        public HtmlInjectionMiddleware(RequestDelegate next, ILogger<HtmlInjectionMiddleware> logger)
        {
            _next = next;
            _logger = logger;
        }

        public async Task InvokeAsync(HttpContext context)
        {
            // Wir interessieren uns nur für die Hauptseite des Web-Clients
            if (!IsIndexPageRequest(context.Request.Path))
            {
                await _next(context);
                return;
            }

            // Wir tauschen den Response-Stream aus, um die Antwort lesen und verändern zu können
            var originalBodyStream = context.Response.Body;
            using var responseBody = new MemoryStream();
            context.Response.Body = responseBody;

            try
            {
                // Die Pipeline weiterlaufen lassen (Jellyfin generiert die index.html)
                await _next(context);

                // Prüfen, ob es wirklich HTML ist
                if (context.Response.ContentType != null && 
                    context.Response.ContentType.ToLower().Contains("text/html"))
                {
                    responseBody.Seek(0, SeekOrigin.Begin);
                    var text = await new StreamReader(responseBody).ReadToEndAsync();

                    // HIER PASSIERT DIE MAGIC:
                    // Wir fügen unser Script direkt vor dem schließenden </body> Tag ein.
                    // Wir nutzen den sauberen Pfad aus dem KindleResourceController.
                    var scriptTag = "<script src=\"/KindlePlugin/ClientScript\" defer></script>";
                    
                    if (!text.Contains("KindlePlugin/ClientScript")) // Verhindert doppeltes Einfügen
                    {
                        var modifiedText = text.Replace("</body>", $"{scriptTag}</body>");
                        
                        // Den modifizierten Text zurückschreiben
                        var modifiedBytes = Encoding.UTF8.GetBytes(modifiedText);
                        context.Response.Body = originalBodyStream;
                        context.Response.ContentLength = modifiedBytes.Length;
                        await originalBodyStream.WriteAsync(modifiedBytes, 0, modifiedBytes.Length);
                    }
                    else
                    {
                        // Falls schon vorhanden, einfach original zurückschreiben
                        responseBody.Seek(0, SeekOrigin.Begin);
                        await responseBody.CopyToAsync(originalBodyStream);
                    }
                }
                else
                {
                    // Kein HTML? Dann einfach durchreichen.
                    responseBody.Seek(0, SeekOrigin.Begin);
                    await responseBody.CopyToAsync(originalBodyStream);
                }
            }
            finally
            {
                // Sicherheitshalber den Body zurücksetzen, falls was schief ging
                context.Response.Body = originalBodyStream;
            }
        }

        private bool IsIndexPageRequest(PathString path)
        {
            if (!path.HasValue) return false;
            var p = path.Value.ToLower();
            // Prüfen auf /web/index.html oder Root /
            return p.EndsWith("/index.html") || p.Equals("/") || p.Equals("/web/");
        }
    }
}