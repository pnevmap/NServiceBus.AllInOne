using System;
using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace Shared.Web.Bootstrapping.FeatureInstallers.Swagger
{
    public class SwaggerInstaller : 
        IInstaller,
        IRegisterServices,
        IBuildMvc,
        IInstallMiddleware
    {
        private readonly Installers _installers;
        public const string SectionName = "ApiDefinitionOptions";
        
        public Type StartupType { get; }
        
        public SwaggerApiDefinition Definition { get; }

        public SwaggerInstaller(Installers installers, SwaggerApiDefinition definition, Type startupType)
        {
            _installers = installers;
            Definition = definition ?? throw new ArgumentNullException(nameof(definition));
            StartupType = startupType;
        }
        
        public static SwaggerInstaller FromConfiguration(Installers installers, IConfiguration configuration, Type startupType)
        {
            var options = configuration.GetSection(SectionName)?.Get<ApiDefinitionOptions>() ?? new ApiDefinitionOptions();
            if (!options.IsEnabled)
                return null;

            var definition = new SwaggerApiDefinition(options, startupType);
            
            return new SwaggerInstaller(installers, definition, startupType);
        }

        int IRegisterServices.Order => 800;

        public void RegisterServices(IServiceCollection services, IConfiguration configuration)
        {
            services.AddSwaggerServices(Definition, _installers);
        }

        int IBuildMvc.Order => 800;

        public void BuildMvc(IMvcBuilder mvcBuilder, IConfiguration configuration)
        {
            mvcBuilder.AddSwaggerDocumentGroupingConvention(Definition);
        }

        int IInstallMiddleware.Order => 800;

        public void InstallMiddleware(IApplicationBuilder app, IServiceProvider serviceProvider)
        {
            app.UseSwaggerUi(Definition);
        }
    }
}