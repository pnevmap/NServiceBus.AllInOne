using System;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Server.Kestrel.Core;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace Shared.Web.Bootstrapping.FeatureInstallers
{
    public interface IInstaller
    {
    }

    public interface IConfigureKestrel
    {
        int Order { get; }
        void ConfigureKestrel(KestrelServerOptions options);
    }
    
    public interface IRegisterServices
    {
        int Order { get; }
        void RegisterServices(IServiceCollection services, IConfiguration configuration);
    }

    public interface IBuildMvc
    {
        int Order { get; }
        void BuildMvc(IMvcBuilder mvcBuilder, IConfiguration configuration);
    }

    public interface IInstallMiddleware
    {
        int Order { get; }
        void InstallMiddleware(IApplicationBuilder app, IServiceProvider serviceProvider);
    }
}