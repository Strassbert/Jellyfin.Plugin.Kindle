using System;
using MediaBrowser.Common.Configuration;
using MediaBrowser.Common.Plugins;
using MediaBrowser.Model.Serialization;
using Jellyfin.Plugin.Kindle.Configuration;

namespace Jellyfin.Plugin.Kindle
{
    public class Plugin : BasePlugin<PluginConfiguration>
    {
        public override string Name => "Kindle Share";
        public override Guid Id => Guid.Parse("E3B2B4A1-1234-4567-89AB-CDEF12345678");

        // FIX: = null! unterdrückt die Warnung. 
        // Wir wissen, dass der Konstruktor (unten) dies sofort setzt.
        public static Plugin Instance { get; private set; } = null!;

        public Plugin(IApplicationPaths applicationPaths, IXmlSerializer xmlSerializer)
            : base(applicationPaths, xmlSerializer)
        {
            Instance = this;
        }
    }
}