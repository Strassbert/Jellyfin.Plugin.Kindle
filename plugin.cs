using System;
using System.Collections.Generic;
using Microsoft.Extensions.Logging; // WICHTIG FÜR LOGS
using MediaBrowser.Common.Configuration;
using MediaBrowser.Common.Plugins;
using MediaBrowser.Model.Plugins;
using MediaBrowser.Model.Serialization;
using Jellyfin.Plugin.Kindle.Configuration;

namespace Jellyfin.Plugin.Kindle
{
    public class Plugin : BasePlugin<PluginConfiguration>, IHasPluginConfiguration
    {
        public override string Name => "Kindle Share";
        public override Guid Id => Guid.Parse("E3B2B4A1-1234-4567-89AB-CDEF12345678");

        private readonly ILogger<Plugin> _logger;

        // Wir lassen uns den Logger injizieren!
        public Plugin(IApplicationPaths applicationPaths, IXmlSerializer xmlSerializer, ILogger<Plugin> logger)
            : base(applicationPaths, xmlSerializer)
        {
            Instance = this;
            _logger = logger;

            // SOFORT LOGGEN (Try-Catch um sicherzugehen)
            try 
            {
                _logger.LogInformation("##################################################");
                _logger.LogInformation("[KINDLE] PLUGIN KONSTRUKTOR WIRD AUSGEFÜHRT!");                
                _logger.LogInformation("##################################################");
            }
            catch (Exception ex)
            {
                // Falls was schief geht, sehen wir das jetzt
                _logger.LogError(ex, "[KINDLE] CRITICAL ERROR IM KONSTRUKTOR!");
            }
        }

        public static Plugin Instance { get; private set; } = null!;

        public IEnumerable<PluginPageInfo> GetPages()
        {
            return new[]
            {
                new PluginPageInfo
                {
                    Name = "KindleSettings",
                    // Dies muss mit dem Log übereinstimmen!
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
                    Name = "kindleButton.js",
                    EmbeddedResourcePath = "Jellyfin.Plugin.Kindle.Web.kindleButton.js",
                    EnableInMainMenu = false
                }
            };
        }
    }
}