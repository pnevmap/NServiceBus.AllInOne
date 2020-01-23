using Microsoft.Extensions.Configuration;
using Serilog;

namespace Shared.Hosting.Abstractions.Logging
{
    public static class SerilogExtensions
    {
        public static ILogger CreateNewLogger(this IConfiguration configuration, string applicationName)
        {
            var logConfig = new LoggerConfiguration();
                
            BuildLoggerConfiguration(logConfig, configuration, applicationName);    

            return logConfig.CreateLogger();
        }        
        
        public static void BuildLoggerConfiguration(this LoggerConfiguration logConfig, IConfiguration configuration, string applicationName)
        {
            var defaults = Defaults.Read(configuration);

            if (defaults.LoggingPolicy?.IsDefined() == true)
            {
                defaults.LoggingPolicy.Configure(logConfig, configuration, applicationName);
            }
            else
            {
                logConfig.ReadFrom.Configuration(configuration);
            }
            
            logConfig
                .Enrich.FromLogContext()
                .Enrich.WithProperty("Application", applicationName);
        }        
    }
}