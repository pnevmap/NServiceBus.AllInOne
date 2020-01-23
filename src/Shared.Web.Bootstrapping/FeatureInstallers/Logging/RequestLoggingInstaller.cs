using System;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Shared.Web.Bootstrapping.Middleware.Logging;

namespace Shared.Web.Bootstrapping.FeatureInstallers.Logging
{
    public class RequestLoggingInstaller : 
        IInstaller,
        IBuildMvc,
        IInstallMiddleware
    {
        int IInstallMiddleware.Order => 400;

        public void InstallMiddleware(IApplicationBuilder app, IServiceProvider serviceProvider)
        {
            var env = serviceProvider.GetService<IHostingEnvironment>();
            
            app.UseMiddleware(env?.IsDevelopment() == true
                ? typeof(DevelopmentLoggingMiddleware)
                : typeof(LoggingMiddleware));
        }

        int IBuildMvc.Order => 400;

        public void BuildMvc(IMvcBuilder mvcBuilder, IConfiguration configuration)
        {
            mvcBuilder.AddMvcOptions(o =>
            {
                o.Filters.Insert(0, new RouteInterceptorActionFilter());
            });
        }
    }
}