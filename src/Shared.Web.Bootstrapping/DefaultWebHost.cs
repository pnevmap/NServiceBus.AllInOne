using Microsoft.AspNetCore;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.DependencyInjection;
using Serilog;
using Serilog.Core;
using Shared.Hosting.Abstractions.Logging;
using Shared.Web.Bootstrapping.FeatureInstallers;

namespace Shared.Web.Bootstrapping
{
    public static class DefaultWebHost
    {
        public static IWebHostBuilder CreateWebHostBuilder<TStartup>(string[] args) where TStartup : class
        {
            var builder = WebHost.CreateDefaultBuilder(args);

            builder = Log.Logger == Logger.None
                ? builder.UseSerilog((h, c) => c.BuildLoggerConfiguration(h.Configuration, typeof(TStartup).Assembly.GetName().Name))
                : builder.UseSerilog(Log.Logger);

            return builder
                .ConfigureKestrel(options =>
                {
                    options.AddServerHeader = false;

                    options.ApplicationServices
                        .GetRequiredService<Installers>()
                        .ConfigureKestrel(options);
                })
                .UseStartup<TStartup>();
        }
    }
}