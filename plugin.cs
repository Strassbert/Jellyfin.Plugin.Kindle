using System;
using System.Collections.Generic;
using MediaBrowser.Common.Configuration;
using MediaBrowser.Common.Plugins;
using MediaBrowser.Model.Plugins;
using MediaBrowser.Model.Serialization;
using Jellyfin.Plugin.Kindle.Configuration;

namespace Jellyfin.Plugin.Kindle
{
    // Wir erben von BasePlugin UND implementieren IHasPluginConfiguration
    public class Plugin : BasePlugin<PluginConfiguration>, IHasPluginConfiguration
    {
        public override string Name => "Kindle Share";
        public override Guid Id => Guid.Parse("E3B2B4A1-1234-4567-89AB-CDEF12345678");

        public Plugin(IApplicationPaths applicationPaths, IXmlSerializer xmlSerializer)
            : base(applicationPaths, xmlSerializer)
        {
            Instance = this;

            // --- DEBUGGING ANFANG ---
    var assembly = System.Reflection.Assembly.GetExecutingAssembly();
    var resourceNames = assembly.GetManifestResourceNames();
    
    System.Console.WriteLine("##################################################");
    System.Console.WriteLine($"[KINDLE CHECK] Plugin Loaded.");
    System.Console.WriteLine($"[KINDLE CHECK] DLL GUID: {Id}");
    System.Console.WriteLine($"[KINDLE CHECK] Gefundene Dateien in der DLL ({resourceNames.Length}):");
    foreach (var resource in resourceNames)
    {
        System.Console.WriteLine($"[KINDLE CHECK] -> {resource}");
    }
    System.Console.WriteLine("##################################################");
    // --- DEBUGGING ENDE ---
        }

        public static Plugin Instance { get; private set; } = null!;

        // --- HIER HAT ES GEFEHLT ---
        // Diese Methode sagt Jellyfin: "Hier sind meine HTML/JS Dateien für die Oberfläche."
        public IEnumerable<PluginPageInfo> GetPages()
        {
            return new[]
            {
                // 1. Die HTML-Seite für die Einstellungen
                new PluginPageInfo
                {
                    Name = "KindleSettings", // WICHTIG: Das wird Teil der URL
                    // Muss exakt stimmen: Namespace + Ordner + Dateiname
                    EmbeddedResourcePath = "Jellyfin.Plugin.Kindle.Configuration.configPage.html",
                    EnableInMainMenu = true, // Zeigt den Eintrag "Kindle Share" im Menü
                    MenuIcon = "email"
                },
                
                // 2. Das dazugehörige JavaScript für die Einstellungen
                new PluginPageInfo
                {
                    Name = "KindleSettingsJs",
                    EmbeddedResourcePath = "Jellyfin.Plugin.Kindle.Configuration.configPage.js"
                },

                // 3. Das Script für den Senden-Button (damit es geladen werden kann)
                new PluginPageInfo
                {
                    Name = "kindleButton.js",
                    EmbeddedResourcePath = "Jellyfin.Plugin.Kindle.Web.kindleButton.js",
                    EnableInMainMenu = false // Soll nicht im Menü auftauchen
                }
            };
        }
    }
}