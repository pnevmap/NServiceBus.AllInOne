using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.AspNetCore.Mvc.ApplicationModels;

namespace Shared.Web.Bootstrapping.FeatureInstallers.Swagger
{
    public class ApiExplorerGroupingByNamespaceSuffixConvention : IControllerModelConvention
    {
        private readonly IEnumerable<ApiDocument> _documents;

        public ApiExplorerGroupingByNamespaceSuffixConvention(IEnumerable<ApiDocument> documents)
        {
            _documents = documents;
        }

        public void Apply(ControllerModel controller)
        {
            var document = _documents.FirstOrDefault(d => Belongs(controller, d));
            if(document != null)
                controller.ApiExplorer.GroupName = document.Name;
        }
        
        private static bool Belongs(ControllerModel model, ApiDocument document)
        {
            var controllerNamespace = model.ControllerType.Namespace;
            
            return controllerNamespace != null && controllerNamespace.EndsWith(document.Name, StringComparison.OrdinalIgnoreCase);
        }
    }
}