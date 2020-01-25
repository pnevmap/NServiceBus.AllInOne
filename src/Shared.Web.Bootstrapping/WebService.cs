using System;
using System.Threading;
using Microsoft.AspNetCore;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.DependencyInjection;
using Serilog;
using Serilog.Core;
using Shared.Hosting.Abstractions;
using Shared.Hosting.Abstractions.Logging;
using Shared.Web.Bootstrapping.FeatureInstallers;

namespace Shared.Web.Bootstrapping
{
    public class WebService<TStartup> : IService
        where TStartup : class
    {
        private readonly string[] _args;

        private IWebHost _webHost;

        public WebService(string[] args)
        {
            _args = args;
        }

        public void Start()
        {
            var webHostBuilder = WebHost.CreateDefaultBuilder(_args);

            if (Log.Logger == Logger.None)
                webHostBuilder = webHostBuilder.UseSerilog((h, c) => c.BuildLoggerConfiguration(h.Configuration, typeof(TStartup).Assembly.GetName().Name));
            else
                webHostBuilder = webHostBuilder.UseSerilog(Log.Logger);

            _webHost = webHostBuilder
                .ConfigureKestrel(options =>
                {
                    options.AddServerHeader = false;
                    options.ApplicationServices
                        .GetRequiredService<Installers>()
                        .ConfigureKestrel(options);
                })
                .UseStartup<TStartup>()
                .Build();

            _webHost.Start();
        }

        public void Stop()
        {
            var cancellation = new CancellationTokenSource(TimeSpan.FromSeconds(15));

            _webHost?.StopAsync(cancellation.Token)
                .GetAwaiter()
                .GetResult();
        }
    }
}