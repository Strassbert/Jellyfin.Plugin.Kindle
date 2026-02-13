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
        private const string ScriptTag = "<script src=\"/KindlePlugin/ClientScript\" defer></script>";

        public HtmlInjectionMiddleware(RequestDelegate next, ILogger<HtmlInjectionMiddleware> logger)
        {
            _next = next;
            _logger = logger;
        }

        public async Task InvokeAsync(HttpContext context)
        {
            // Only intercept GET requests to the index page
            if (!HttpMethods.IsGet(context.Request.Method) || !IsIndexPageRequest(context.Request.Path))
            {
                await _next(context);
                return;
            }

            var originalBodyStream = context.Response.Body;
            using var responseBody = new MemoryStream();
            context.Response.Body = responseBody;

            try
            {
                await _next(context);

                // Only modify successful HTML responses
                if (context.Response.StatusCode != 200 ||
                    context.Response.ContentType == null ||
                    !context.Response.ContentType.Contains("text/html", System.StringComparison.OrdinalIgnoreCase))
                {
                    responseBody.Seek(0, SeekOrigin.Begin);
                    await responseBody.CopyToAsync(originalBodyStream);
                    return;
                }

                responseBody.Seek(0, SeekOrigin.Begin);
                var text = await new StreamReader(responseBody, Encoding.UTF8).ReadToEndAsync();

                if (!text.Contains("KindlePlugin/ClientScript"))
                {
                    var modifiedText = text.Replace("</body>", $"{ScriptTag}</body>");
                    var modifiedBytes = Encoding.UTF8.GetBytes(modifiedText);
                    context.Response.Body = originalBodyStream;
                    context.Response.ContentLength = modifiedBytes.Length;
                    await originalBodyStream.WriteAsync(modifiedBytes, 0, modifiedBytes.Length);
                    _logger.LogDebug("[Kindle] Injected client script into index page.");
                }
                else
                {
                    responseBody.Seek(0, SeekOrigin.Begin);
                    context.Response.Body = originalBodyStream;
                    await responseBody.CopyToAsync(originalBodyStream);
                }
            }
            finally
            {
                context.Response.Body = originalBodyStream;
            }
        }

        private static bool IsIndexPageRequest(PathString path)
        {
            if (!path.HasValue) return false;
            var p = path.Value!;
            return p.EndsWith("/index.html", System.StringComparison.OrdinalIgnoreCase)
                || p.Equals("/", System.StringComparison.Ordinal)
                || p.Equals("/web/", System.StringComparison.OrdinalIgnoreCase);
        }
    }
}
