using System;
using System.Collections.Generic;
using MediaBrowser.Common.Configuration;
using MediaBrowser.Common.Plugins;
using MediaBrowser.Model.Plugins;
using MediaBrowser.Model.Serialization;
using Jellyfin.Plugin.Kindle.Configuration;
using Microsoft.Extensions.Logging;

namespace Jellyfin.Plugin.Kindle
{
    public class Plugin : BasePlugin<PluginConfiguration>, IHasPluginConfiguration
    {
        public override string Name => "Kindle Share";
        public override Guid Id => Guid.Parse("E3B2B4A1-1234-4567-89AB-CDEF12345678");
        public override string Description => "Send e-books (EPUB, PDF, MOBI) from Jellyfin to your Amazon Kindle.";

        private readonly ILogger<Plugin> _logger;

        public Plugin(IApplicationPaths applicationPaths, IXmlSerializer xmlSerializer, ILogger<Plugin> logger)
            : base(applicationPaths, xmlSerializer)
        {
            Instance = this;
            _logger = logger;
            _logger.LogInformation("[Kindle] Plugin initialized.");
        }

        public static Plugin Instance { get; private set; } = null!;

        public IEnumerable<PluginPageInfo> GetPages()
        {
            return new[]
            {
                new PluginPageInfo
                {
                    Name = "KindleSettings",
                    EmbeddedResourcePath = "Jellyfin.Plugin.Kindle.Configuration.configPage.html",
                    EnableInMainMenu = true,
                    MenuIcon = "email"
                },
                new PluginPageInfo
                {
                    Name = "KindleSettingsJs",
                    EmbeddedResourcePath = "Jellyfin.Plugin.Kindle.Configuration.configPage.js"
                },
                new PluginPageInfo
                {
                    Name = "KindleUserSettings",
                    EmbeddedResourcePath = "Jellyfin.Plugin.Kindle.Configuration.userSettings.html",
                    EnableInMainMenu = false
                },
                new PluginPageInfo
                {
                    Name = "KindleUserSettingsJs",
                    EmbeddedResourcePath = "Jellyfin.Plugin.Kindle.Configuration.userSettings.js"
                }
            };
        }
    }
}
