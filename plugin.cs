using System;
using System.Collections.Generic;
using System.Globalization;
using MediaBrowser.Common.Configuration;
using MediaBrowser.Common.Plugins;
using MediaBrowser.Model.Plugins;
using MediaBrowser.Model.Serialization;
using Jellyfin.Plugin.Kindle.Configuration;
using Microsoft.Extensions.Logging;

namespace Jellyfin.Plugin.Kindle
{
    public class Plugin : BasePlugin<PluginConfiguration>, IHasWebPages
    {
        // Name and Description must match manifest.json exactly. Jellyfin rewrites
        // meta.json from the running instance (PluginManager.CreatePluginInstance),
        // so a mismatch makes the on-disk manifest drift away from the repository
        // manifest and breaks version matching for updates and removal.
        public const string PluginName = "E-Book Share";

        public override string Name => PluginName;

        public override Guid Id => Guid.Parse("E3B2B4A1-1234-4567-89AB-CDEF12345678");

        public override string Description =>
            "Send e-books (EPUB, PDF, MOBI, AZW3) directly from the detail page to an E-Book Reader via email.";

        private readonly ILogger<Plugin> _logger;

        public Plugin(IApplicationPaths applicationPaths, IXmlSerializer xmlSerializer, ILogger<Plugin> logger)
            : base(applicationPaths, xmlSerializer)
        {
            Instance = this;
            _logger = logger;

            Configuration.Migrate();

            _logger.LogInformation("[E-Book Share] Plugin v{Version} initialized.", Version);
        }

        public static Plugin? Instance { get; private set; }

        /// <summary>
        /// Cache-busting token for the injected client script. Changes with every
        /// build so browsers cannot keep serving the previous version's script.
        /// </summary>
        public string ClientScriptVersion => Version?.ToString() ?? "0";

        public override void OnUninstalling()
        {
            // Nothing to undo on disk: the client script is injected per request by
            // HtmlInjectionMiddleware rather than written into index.html, so it
            // disappears together with the plugin. The saved configuration is left
            // in place deliberately, matching Jellyfin's behaviour for every other
            // plugin, so a reinstall keeps the SMTP settings and user addresses.
            _logger.LogInformation("[E-Book Share] Uninstalling. Configuration file is kept at {Path}.", ConfigurationFilePath);
            base.OnUninstalling();
        }

        public IEnumerable<PluginPageInfo> GetPages()
        {
            return new[]
            {
                new PluginPageInfo
                {
                    Name = "KindleSettings",
                    DisplayName = PluginName,
                    EmbeddedResourcePath = string.Format(CultureInfo.InvariantCulture, "{0}.Configuration.configPage.html", GetType().Namespace),
                    EnableInMainMenu = true,
                    MenuIcon = "menu_book"
                },
                new PluginPageInfo
                {
                    Name = "KindleUserSettings",
                    DisplayName = PluginName,
                    EmbeddedResourcePath = string.Format(CultureInfo.InvariantCulture, "{0}.Configuration.userSettings.html", GetType().Namespace),
                    EnableInMainMenu = false
                }
            };
        }
    }
}
