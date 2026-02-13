using System;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.DependencyInjection;
using MediaBrowser.Controller;
using MediaBrowser.Controller.Plugins;
using Jellyfin.Plugin.Kindle.Services;

namespace Jellyfin.Plugin.Kindle
{
    public class PluginStartup : IStartupFilter
    {
        public Action<IApplicationBuilder> Configure(Action<IApplicationBuilder> next)
        {
            return builder =>
            {
                builder.UseMiddleware<Configuration.HtmlInjectionMiddleware>();
                next(builder);
            };
        }
    }

    public class PluginServiceRegistrator : IPluginServiceRegistrator
    {
        public void RegisterServices(IServiceCollection serviceCollection, IServerApplicationHost applicationHost)
        {
            serviceCollection.AddSingleton<KindleMailService>();
            serviceCollection.AddTransient<IStartupFilter, PluginStartup>();
        }
    }
}
