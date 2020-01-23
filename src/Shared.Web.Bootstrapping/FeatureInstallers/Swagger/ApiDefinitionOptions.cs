using System.Collections.Generic;

namespace Shared.Web.Bootstrapping.FeatureInstallers.Swagger
{
    public class ApiDocument
    {
        public string Name { get; set; }
        public string Title { get; set; }
        public string Version { get; set; }
    }
    public class ApiDefinitionOptions
    {
        public bool IsEnabled { get; set; } = true;
        public List<ApiDocument> Documents { get; } = new List<ApiDocument>();
        public List<string> XmlCommentsFiles { get; } = new List<string>();
        public SecurityDefinitionKeyScheme SecurityDefinitionKeyScheme { get; set; } = SecurityDefinitionKeyScheme.Default;
        public DocumentGroupingPolicyType DocumentGroupingPolicy { get; set; } = DocumentGroupingPolicyType.ByNamespaceSuffix;
        public GroupActionsBy GroupActionsBy { get; set; } = GroupActionsBy.Controller;
        public SchemaIdType SchemaId { get; set; } = SchemaIdType.Default;
    }

    public enum DocumentGroupingPolicyType
    {
        Default = 0,
        ByNamespaceSuffix = 1
    }

    public enum GroupActionsBy
    {
        None = 0,
        Controller = 1,
        RelativePath = 2,
        HttpMethod = 3,
        ControllerFullName = 4
    }

    public enum SchemaIdType
    {
        Default = 0,
        FullName = 1
    }

    public enum SecurityDefinitionKeyScheme
    {
        Default = 0,
        None = 1,
        Bearer = 2,
        Oauth2 = 3
    }
}