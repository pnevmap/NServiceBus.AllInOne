using System;
using Topshelf;
using Topshelf.HostConfigurators;

namespace Shared.Hosting.Abstractions
{
    public class ServiceHostingControl
    {
        public HostControl HostControl { get; set; }

        public void Stop()
        {
            HostControl?.Stop();
        }

        public Action<HostConfigurator> CustomConfiguration { get; set; }
    }
}