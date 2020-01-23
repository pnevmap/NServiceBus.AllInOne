using System;
using System.Collections.Generic;
using System.IO;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Mvc.ApiExplorer;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.PlatformAbstractions;
using Swashbuckle.AspNetCore.Swagger;
using Swashbuckle.AspNetCore.SwaggerGen;

namespace Shared.Web.Bootstrapping.FeatureInstallers.Swagger
{
    public static class SwaggerExtensions
    {
        public static void AddSwaggerServices(this IServiceCollection services, SwaggerApiDefinition swagger, Installers installers)
        {
            if (!swagger.Options.IsEnabled)
                return;

            services.AddSwaggerGen(c =>
            {
                foreach (var document in swagger.Options.Documents)
                {
                    c.SwaggerDoc(document.Name, new Info {Title = document.Title ?? document.Name, Version = document.Version});
                }

                c.DescribeAllEnumsAsStrings();
                c.DescribeStringEnumsInCamelCase();

                ApplyGroupingBy(swagger.Options.GroupActionsBy, c);

                foreach (var commentsFile in swagger.Options.XmlCommentsFiles)
                {
                    var filePath = Path.Combine(PlatformServices.Default.Application.ApplicationBasePath, commentsFile);
                    c.IncludeXmlComments(filePath);
                }
                

                if (swagger.Options.SchemaId != SchemaIdType.Default)
                {
                    ConfigureCustomSchemaId(swagger.Options.SchemaId, c);
                }
            });
        }

        private static void ConfigureCustomSchemaId(SchemaIdType optionsSchemaId, SwaggerGenOptions swaggerGenOptions)
        {
            switch (optionsSchemaId)
            {
                case SchemaIdType.FullName:
                    swaggerGenOptions.CustomSchemaIds(type => type.FullName);
                    break;
            }
        }

        private static void ApplyGroupingBy(GroupActionsBy groupActionsBy, SwaggerGenOptions options)
        {
            switch (groupActionsBy)
            {
                case GroupActionsBy.Controller:
                    options.TagActionsBy(a => new List<string> {GetControllerName(a)});
                    break;
                case GroupActionsBy.RelativePath:
                    options.TagActionsBy(a => new List<string> {a.RelativePath});
                    break;
                case GroupActionsBy.HttpMethod:
                    options.TagActionsBy(a => new List<string> {a.HttpMethod});
                    break;
                case GroupActionsBy.ControllerFullName:
                    options.TagActionsBy(a => new List<string> {GetActionDisplayName(a)});
                    break;
            }
        }

        private static string GetControllerName(ApiDescription api)
        {
            return api.ActionDescriptor.RouteValues.TryGetValue("controller", out var name) && !string.IsNullOrWhiteSpace(name)
                ? name
                : api.ActionDescriptor.DisplayName;
        }

        private static string GetActionDisplayName(ApiDescription api)
        {
            var name = api.ActionDescriptor.DisplayName;

            var idx = name.IndexOf("(", StringComparison.OrdinalIgnoreCase);
            if (idx > 0)
                name = name.Substring(0, idx);

            idx = name.LastIndexOf(".", StringComparison.OrdinalIgnoreCase);

            if (idx > 0)
                name = name.Substring(0, idx);

            return name.Length > 0
                ? name
                : api.ActionDescriptor.DisplayName;
        }

        public static void AddSwaggerDocumentGroupingConvention(this IMvcBuilder mvcBuilder, SwaggerApiDefinition swagger)
        {
            if (!swagger.Options.IsEnabled)
                return;

            if (swagger.Options.DocumentGroupingPolicy == DocumentGroupingPolicyType.ByNamespaceSuffix)
                mvcBuilder.AddMvcOptions(o => o.Conventions.Add(new ApiExplorerGroupingByNamespaceSuffixConvention(swagger.Options.Documents)));
        }

        public static void UseSwaggerUi(this IApplicationBuilder app, SwaggerApiDefinition swagger)
        {
            if (!swagger.Options.IsEnabled)
                return;

            app.UseSwagger();

            app.UseSwaggerUI(c =>
            {
                foreach (var document in swagger.Options.Documents)
                {
                    c.SwaggerEndpoint($"/swagger/{document.Name}/swagger.json", $"{document.Title} {document.Version}");
                }
            });
        }
    }
}