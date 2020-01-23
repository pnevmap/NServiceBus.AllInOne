using System;
using System.Reflection;
using AutoMapper;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Serilog;
using Shared.Hosting.Abstractions;
using Shared.Web.Bootstrapping.FeatureInstallers;

namespace Shared.Web.Bootstrapping
{
    public abstract class WebHosStartup : WebHosStartup<InstallersBuilder>
    {
        protected WebHosStartup(IConfiguration configuration)
            : base(configuration)
        {
        }
    }

    public abstract class WebHosStartup<TInstallerBuilder> where TInstallerBuilder : IInstallersBuilder, new()
    {
        protected Installers Installers { get; private set; }
        protected IConfiguration Configuration { get; }
        protected Defaults Defaults { get; set; }

        protected WebHosStartup(IConfiguration configuration)
        {
            Configuration = configuration ?? throw new ArgumentNullException(nameof(configuration));
            
            Defaults = Defaults.Read(Configuration);

            Installers = new TInstallerBuilder().Build(GetType(), Configuration);
        }

        public void ConfigureServices(IServiceCollection services)
        {
            Log.Logger.Information($"Starting up {GetType().FullName}");
            
            services.AddSingleton(Installers);
            
            services.AddSingleton(Defaults);
            
            services.AddSingleton(Log.Logger);
            
            services.AddSingleton<IHttpContextAccessor, HttpContextAccessor>();

            services.AddAutoMapper(GetType().GetTypeInfo().Assembly);

            Installers.RegisterServices(services, Configuration);
            
            ConfigureComponents(services);

            ConfigureMvc(services);
            
        }

        public void Configure(IApplicationBuilder app, IServiceProvider serviceProvider)
        {
            Installers.InstallMiddleware(app, serviceProvider);
            
            UseMvc(app, serviceProvider);
        }

        private void ConfigureMvc(IServiceCollection services)
        {
            var builder = services
                .AddMvc()
                .AddMvcOptions(o =>
                {
                    o.InputFormatters.Add(new TextPlainInputFormatter());
                });
            
            Installers.BuildMvc(builder, Configuration);
            
            BuildMvc(builder);
            
            builder.SetCompatibilityVersion(CompatibilityVersion.Version_2_2);
        }

        protected virtual void ConfigureComponents(IServiceCollection services)
        {
            // Intentional
        }

        protected virtual void BuildMvc(IMvcBuilder mvcBuilder)
        {
            // Intentional
        }
        
        protected virtual void UseMvc(IApplicationBuilder app, IServiceProvider serviceProvider)
        {
            app.UseMvc();
        }
    }
}