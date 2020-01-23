namespace Shared.Web.Bootstrapping.FeatureInstallers.HealthCheck
{
    public class HealthCheckOptions
    {
        public string HealthProbeUrl { get; set; } = "/" ;
        public string ApplicationDbHealthProbeUrl { get; set; } = "/db";
        public bool CheckDbConnectivity { get; set; } = true;
    }
}