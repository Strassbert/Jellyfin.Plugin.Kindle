using System;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.DependencyInjection;
using MediaBrowser.Controller;
using MediaBrowser.Controller.Plugins;
using Jellyfin.Plugin.Kindle.Configuration;
using Jellyfin.Plugin.Kindle.Services;

namespace Jellyfin.Plugin.Kindle
{
    public class PluginStartup : IStartupFilter
    {
        public Action<IApplicationBuilder> Configure(Action<IApplicationBuilder> next)
        {
            return builder =>
            {
                builder.UseMiddleware<HtmlInjectionMiddleware>();
                next(builder);
            };
        }
    }

    public class PluginServiceRegistrator : IPluginServiceRegistrator
    {
        public void RegisterServices(IServiceCollection serviceCollection, IServerApplicationHost applicationHost)
        {
            // Register PluginConfiguration as singleton (obtained from Plugin.Instance)
            serviceCollection.AddSingleton<PluginConfiguration>(provider =>
            {
                var pluginInstance = Plugin.Instance;
                if (pluginInstance == null)
                    throw new InvalidOperationException("Kindle Plugin instance not initialized");
                return pluginInstance.Configuration;
            });

            // Register KindleMailService with PluginConfiguration injected
            serviceCollection.AddSingleton<KindleMailService>();

            // Register Rate Limiting Service
            serviceCollection.AddSingleton<RateLimitingService>();

            // Register Security Service for password encryption
            serviceCollection.AddSingleton<KindleSecurityService>();

            // Register Startup filter for middleware
            serviceCollection.AddTransient<IStartupFilter, PluginStartup>();
        }
    }
}
