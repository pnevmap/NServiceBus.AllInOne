using System;
using System.Threading;
using Microsoft.AspNetCore.Hosting;
using Shared.Hosting.Abstractions;

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
            _webHost = DefaultWebHost.CreateWebHostBuilder<TStartup>(_args).Build();
            
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