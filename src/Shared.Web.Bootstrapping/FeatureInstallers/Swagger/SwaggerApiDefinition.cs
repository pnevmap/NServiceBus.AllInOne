using System;
using System.Linq;

namespace Shared.Web.Bootstrapping.FeatureInstallers.Swagger
{
    public class SwaggerApiDefinition
    {
        public ApiDefinitionOptions Options { get; }
        
        public SwaggerApiDefinition(ApiDefinitionOptions options, Type setupType)
        {
            Options = options;

            if(Options.IsEnabled && Options.Documents.All(x => string.IsNullOrWhiteSpace(x.Name)))
            {
                var assemblyName = setupType.Assembly.GetName();
                
                Options.Documents.Add(new ApiDocument
                {
                    Name = assemblyName.Name,
                    Title = assemblyName.Name,
                    Version = assemblyName.Version.ToString()
                });

                Options.DocumentGroupingPolicy = DocumentGroupingPolicyType.Default;
            }
        }
    }
}