using System;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.DependencyInjection;
// Wir importieren die Parents, damit der Compiler den Typ selbst sucht
using MediaBrowser.Controller;
using MediaBrowser.Controller.Plugins;
using MediaBrowser.Common;
using Jellyfin.Plugin.Kindle.Services;

namespace Jellyfin.Plugin.Kindle
{
    // 1. Die Middleware Startup-Klasse
    public class PluginStartup : IStartupFilter
    {
        public Action<IApplicationBuilder> Configure(Action<IApplicationBuilder> next)
        {
            return builder =>
            {
                // Hier aktivieren wir unsere Injection-Middleware
                builder.UseMiddleware<Jellyfin.Plugin.Kindle.Configuration.HtmlInjectionMiddleware>();
                next(builder);
            };
        }
    }

    // 2. Der Service Registrator
    public class PluginServiceRegistrator : IPluginServiceRegistrator
    {
        // TRICK: Wir nutzen hier den Typenamen ohne expliziten Namespace-Import.
        // Da wir "MediaBrowser.Controller" oben drin haben, sollte er es finden.
        // Falls er meckert, ist die DLL kaputt (siehe Schritt 1).
        public void RegisterServices(IServiceCollection serviceCollection, IServerApplicationHost applicationHost)
        {
            // Registriere den Mail-Service (Backend)
            serviceCollection.AddSingleton<KindleMailService>();

            // Registriere die Middleware (Frontend Injection)
            serviceCollection.AddTransient<IStartupFilter, PluginStartup>();
        }
    }
}