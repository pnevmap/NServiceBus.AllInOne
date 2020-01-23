using System;
using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.Configuration;

namespace Shared.Web.Bootstrapping.FeatureInstallers.HeaderForwarding
{
    public class HeaderForwardingInstaller : IInstaller, IInstallMiddleware
    {
        int IInstallMiddleware.Order { get; } = 100;

        public void InstallMiddleware(IApplicationBuilder app, IServiceProvider serviceProvider)
        {
            app.UseForwardedHeaders();
        }
    }
}