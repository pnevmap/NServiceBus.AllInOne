using System;
using Microsoft.Extensions.Configuration;
using Shared.Web.Bootstrapping.FeatureInstallers.HeaderForwarding;
using Shared.Web.Bootstrapping.FeatureInstallers.HealthCheck;
using Shared.Web.Bootstrapping.FeatureInstallers.Logging;
using Shared.Web.Bootstrapping.FeatureInstallers.Swagger;

namespace Shared.Web.Bootstrapping.FeatureInstallers
{
    public class InstallersBuilder : IInstallersBuilder
    {
        public virtual Installers Build(Type startupType, IConfiguration configuration)
        {
            var installers = new Installers();

            installers.Add(new HeaderForwardingInstaller());

            var healthCheck = HealthCheckInstaller.FromConfiguration(configuration);
            if (healthCheck != null)
                installers.Add(healthCheck);

            installers.Add(new RequestLoggingInstaller());

            var swagger = SwaggerInstaller.FromConfiguration(installers, configuration, startupType);
            if (swagger != null)
                installers.Add(swagger);

            return installers;
        }
    }
}