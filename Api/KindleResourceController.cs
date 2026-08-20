using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.IO;
using System.Reflection;
using System.Text.Json;
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
        private const string ScriptResource = "Jellyfin.Plugin.Kindle.Web.kindleButton.js";
        private const string FallbackLanguage = "en";

        // Parsed once per language and reused; the files are embedded so they cannot
        // change without a new assembly.
        private static readonly ConcurrentDictionary<string, IReadOnlyDictionary<string, string>> StringCache = new();

        /// <summary>
        /// Serves the client script referenced from index.html. Anonymous on purpose:
        /// index.html itself is served before login and the script contains no secrets,
        /// it only calls authenticated endpoints once a session exists.
        /// </summary>
        [HttpGet("ClientScript")]
        public ActionResult GetClientScript()
        {
            var stream = Assembly.GetExecutingAssembly().GetManifestResourceStream(ScriptResource);

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

        /// <summary>
        /// Serves the UI strings for one language. The single source of truth for the
        /// injected button, the admin page and the user page, which previously each
        /// carried their own copy of the same tables.
        /// </summary>
        [HttpGet("Strings")]
        public ActionResult<IReadOnlyDictionary<string, string>> GetStrings([FromQuery] string? lang)
        {
            Response.Headers[HeaderNames.CacheControl] = "public, max-age=31536000, immutable";
            return Ok(GetMergedStrings(lang));
        }

        internal static IReadOnlyDictionary<string, string> GetMergedStrings(string? lang)
        {
            var code = Normalize(lang);

            return StringCache.GetOrAdd(code, static key =>
            {
                var merged = new Dictionary<string, string>(Load(FallbackLanguage), StringComparer.Ordinal);

                if (!string.Equals(key, FallbackLanguage, StringComparison.Ordinal))
                {
                    // Overlay rather than replace, so a translation that is missing a
                    // key falls back to English instead of rendering the raw key.
                    foreach (var pair in Load(key))
                    {
                        merged[pair.Key] = pair.Value;
                    }
                }

                return merged;
            });
        }

        private static string Normalize(string? lang)
        {
            if (string.IsNullOrWhiteSpace(lang))
            {
                return FallbackLanguage;
            }

            // Accepts "de", "de-DE" and "de_DE" alike.
            var code = lang.Trim().Replace('_', '-');
            var dash = code.IndexOf('-', StringComparison.Ordinal);
            if (dash > 0)
            {
                code = code[..dash];
            }

            code = code.ToLowerInvariant();

            return ResourceExists(code) ? code : FallbackLanguage;
        }

        private static string ResourceName(string code) =>
            $"Jellyfin.Plugin.Kindle.Localization.{code}.json";

        private static bool ResourceExists(string code)
        {
            if (code.Length != 2 || !char.IsAsciiLetterLower(code[0]) || !char.IsAsciiLetterLower(code[1]))
            {
                return false;
            }

            using var stream = Assembly.GetExecutingAssembly().GetManifestResourceStream(ResourceName(code));
            return stream is not null;
        }

        private static IReadOnlyDictionary<string, string> Load(string code)
        {
            using var stream = Assembly.GetExecutingAssembly().GetManifestResourceStream(ResourceName(code));
            if (stream is null)
            {
                return new Dictionary<string, string>(StringComparer.Ordinal);
            }

            using var reader = new StreamReader(stream);
            var json = reader.ReadToEnd();

            return JsonSerializer.Deserialize<Dictionary<string, string>>(json)
                   ?? new Dictionary<string, string>(StringComparer.Ordinal);
        }
    }
}
