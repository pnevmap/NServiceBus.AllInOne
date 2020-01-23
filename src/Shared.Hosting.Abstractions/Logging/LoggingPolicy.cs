using System.Collections.Generic;
using System.Linq;
using Microsoft.Extensions.Configuration;
using Serilog;
using Serilog.Events;

namespace Shared.Hosting.Abstractions.Logging
{
    public class LoggingPolicy : ILoggingPolicy
    {
        public string Section { get; set; }
    
        public string OutputTemplate { get; set; }
        public MinimumLevelOptions MinimumLevel { get; } = new MinimumLevelOptions();
        
        public class MinimumLevelOptions
        {
            public LogEventLevel Default { get; set; } = LogEventLevel.Verbose;

            public Dictionary<string, LogEventLevel> Override { get; } = new Dictionary<string, LogEventLevel>();
        }
        
        public string Path { get; set; } = "./logs/{{{ApplicationName}}}-{Hour}.log";

        public bool IsDefined()
        {
            return !string.IsNullOrWhiteSpace(Section) ||
                   !string.IsNullOrWhiteSpace(Path);
        }

        public void Configure(LoggerConfiguration logConfig, IConfiguration configuration, string applicationName)
        {
            if (!string.IsNullOrWhiteSpace(Section))
            {
                logConfig
                    .ReadFrom.Configuration(configuration, Section);
                return;                
            }
            
            var path = Path.Replace("{{{ApplicationName}}}", applicationName);
                
            logConfig
                .MinimumLevel.Is(MinimumLevel.Default)
                .WriteTo.Async(a =>
                {
                    if (IsJsonOutput())
                    {
                        a.Console(outputTemplate: "{Timestamp:yyyy-MM-dd HH:mm:ss.fff} [{Level}] <{SourceContext}> {Message}{NewLine}{Exception}");
                        a.RollingFile(pathFormat: path, shared: true, formatter: new Serilog.Formatting.Compact.RenderedCompactJsonFormatter());
                    }
                    else
                    {
                        a.Console(outputTemplate: OutputTemplate);
                        a.RollingFile(pathFormat: path, shared: true, outputTemplate: OutputTemplate);
                    }
                });

            if (MinimumLevel.Override == null || !MinimumLevel.Override.Any())
            {
                logConfig.MinimumLevel.Override("Microsoft", LogEventLevel.Information);
                logConfig.MinimumLevel.Override("Shared.Web.Middleware.Logging", LogEventLevel.Warning);
            }
            else
            {
                foreach (var level in MinimumLevel.Override)
                {
                    logConfig.MinimumLevel.Override(level.Key, level.Value);
                }
            }
        }

        private bool IsJsonOutput()
        {
            return string.IsNullOrWhiteSpace(OutputTemplate) || string.Equals(OutputTemplate, "json");
        }
    }
}