using System;
using System.IO;
using System.Text;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using Microsoft.Net.Http.Headers;

namespace Jellyfin.Plugin.Kindle.Configuration
{
    /// <summary>
    /// Adds the plugin's client script to the web interface by rewriting index.html as
    /// it is served, rather than editing the file on disk. Nothing has to be undone on
    /// uninstall and a Jellyfin update cannot overwrite the change.
    /// </summary>
    public class HtmlInjectionMiddleware
    {
        private const string ScriptPath = "/KindlePlugin/ClientScript";
        private const string Marker = "data-plugin=\"e-book-share\"";

        private readonly RequestDelegate _next;
        private readonly ILogger<HtmlInjectionMiddleware> _logger;

        public HtmlInjectionMiddleware(RequestDelegate next, ILogger<HtmlInjectionMiddleware> logger)
        {
            _next = next;
            _logger = logger;
        }

        public async Task InvokeAsync(HttpContext context)
        {
            if (!HttpMethods.IsGet(context.Request.Method) || !IsIndexPageRequest(context.Request.Path))
            {
                await _next(context).ConfigureAwait(false);
                return;
            }

            var originalBodyStream = context.Response.Body;
            using var buffer = new MemoryStream();

            // Response compression runs later in the pipeline, so ask for an
            // unencoded body - otherwise we would buffer gzip bytes and find no
            // </body> to replace. Restored below for downstream diagnostics.
            var originalAcceptEncoding = context.Request.Headers.AcceptEncoding;
            context.Request.Headers.AcceptEncoding = "identity";

            // index.html itself never changes when the plugin is installed or updated,
            // so its ETag stays the same and the browser would keep revalidating into a
            // 304 - serving a cached page that still points at the previous script
            // version, or at no script at all right after installation. Dropping the
            // validators forces a full response we can actually inject into.
            var originalIfNoneMatch = context.Request.Headers.IfNoneMatch;
            var originalIfModifiedSince = context.Request.Headers.IfModifiedSince;
            context.Request.Headers.Remove(HeaderNames.IfNoneMatch);
            context.Request.Headers.Remove(HeaderNames.IfModifiedSince);

            try
            {
                context.Response.Body = buffer;

                await _next(context).ConfigureAwait(false);

                context.Response.Body = originalBodyStream;
                buffer.Seek(0, SeekOrigin.Begin);

                if (!ShouldInject(context))
                {
                    await buffer.CopyToAsync(originalBodyStream).ConfigureAwait(false);
                    return;
                }

                var html = await new StreamReader(buffer, Encoding.UTF8).ReadToEndAsync().ConfigureAwait(false);

                var closingBody = html.LastIndexOf("</body>", StringComparison.OrdinalIgnoreCase);
                if (closingBody < 0 || html.Contains(Marker, StringComparison.Ordinal))
                {
                    // Nothing to anchor to, or another instance of this middleware
                    // already handled the response.
                    var passthrough = Encoding.UTF8.GetBytes(html);
                    context.Response.ContentLength = passthrough.Length;
                    await originalBodyStream.WriteAsync(passthrough).ConfigureAwait(false);
                    return;
                }

                var modified = html.Insert(closingBody, BuildScriptTag(context));
                var bytes = Encoding.UTF8.GetBytes(modified);

                // The body grew, so the upstream Content-Length no longer applies and
                // the upstream validators describe the un-injected file.
                context.Response.ContentLength = bytes.Length;
                context.Response.Headers.Remove(HeaderNames.ETag);
                context.Response.Headers.Remove(HeaderNames.LastModified);
                context.Response.Headers[HeaderNames.CacheControl] = "no-cache, must-revalidate";
                await originalBodyStream.WriteAsync(bytes).ConfigureAwait(false);

                _logger.LogDebug("[E-Book Share] Injected client script into {Path}.", context.Request.Path);
            }
            finally
            {
                context.Request.Headers.AcceptEncoding = originalAcceptEncoding;
                context.Request.Headers.IfNoneMatch = originalIfNoneMatch;
                context.Request.Headers.IfModifiedSince = originalIfModifiedSince;
                context.Response.Body = originalBodyStream;
            }
        }

        private static bool ShouldInject(HttpContext context)
        {
            if (context.Response.StatusCode != StatusCodes.Status200OK)
            {
                return false;
            }

            var contentType = context.Response.ContentType;
            if (contentType is null || !contentType.Contains("text/html", StringComparison.OrdinalIgnoreCase))
            {
                return false;
            }

            // If something downstream compressed anyway, the buffer is not text and
            // must be passed through untouched.
            return string.IsNullOrEmpty(context.Response.Headers.ContentEncoding);
        }

        /// <summary>
        /// Builds an absolute URL that survives a reverse proxy sub-path. Jellyfin
        /// serves the interface under PathBase when a base URL is configured, so a
        /// hard-coded "/KindlePlugin/..." would 404 on those installations.
        /// </summary>
        private static string BuildScriptTag(HttpContext context)
        {
            var pathBase = context.Request.PathBase.HasValue
                ? context.Request.PathBase.Value!.TrimEnd('/')
                : string.Empty;

            var version = Plugin.Instance?.ClientScriptVersion ?? "0";

            return $"<script {Marker} src=\"{pathBase}{ScriptPath}?v={version}\" defer></script>";
        }

        private static bool IsIndexPageRequest(PathString path)
        {
            if (!path.HasValue)
            {
                return false;
            }

            var p = path.Value!;
            return p.EndsWith("/index.html", StringComparison.OrdinalIgnoreCase)
                || p.Equals("/", StringComparison.Ordinal)
                || p.EndsWith("/web/", StringComparison.OrdinalIgnoreCase)
                || p.EndsWith("/web", StringComparison.OrdinalIgnoreCase);
        }
    }
}
