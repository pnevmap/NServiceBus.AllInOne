using System;
using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Diagnostics.HealthChecks;
using AspNetCoreChecks = Microsoft.AspNetCore.Diagnostics.HealthChecks;
namespace Shared.Web.Bootstrapping.FeatureInstallers.HealthCheck
{
    public class HealthCheckInstaller :
        IInstaller,
        IRegisterServices,
        IInstallMiddleware
    {
        public const string SectionName = "HealthCheckOptions";

        public HealthCheckOptions Options { get; }

        public HealthCheckInstaller(HealthCheckOptions options)
        {
            Options = options ?? throw new ArgumentNullException(nameof(options));

            if (string.IsNullOrWhiteSpace(options.HealthProbeUrl))
                throw new ArgumentException("HealthProbeUrl was not specified");
        }

        public static HealthCheckInstaller FromConfiguration(IConfiguration configuration)
        {
            var options = configuration.GetSection(SectionName)?.Get<HealthCheckOptions>() ??
                          new HealthCheckOptions();

            return string.IsNullOrWhiteSpace(options?.HealthProbeUrl)
                ? null
                : new HealthCheckInstaller(options);
        }

        int IRegisterServices.Order => 200;

        public void RegisterServices(IServiceCollection services, IConfiguration configuration)
        {
            services.AddHealthChecks();
            if (Options.CheckDbConnectivity)
                services.AddHealthChecks()
                    .AddSqlServer(connectionString: configuration.GetConnectionString("Db"), name: "check_application_db");

        }

        int IInstallMiddleware.Order => 200;

        public void InstallMiddleware(IApplicationBuilder app, IServiceProvider serviceProvider)
        {
            app.UseHealthChecks(Options.HealthProbeUrl, new AspNetCoreChecks.HealthCheckOptions { Predicate = check => false });

            app.UseHealthChecks(Options.ApplicationDbHealthProbeUrl, new AspNetCoreChecks.HealthCheckOptions { Predicate = check => check.Name == "check_application_db" });
        }

    }
}