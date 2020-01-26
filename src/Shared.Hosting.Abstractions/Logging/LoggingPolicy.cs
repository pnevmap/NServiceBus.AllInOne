using System.Collections.Generic;
using System.Linq;
using Microsoft.Extensions.Configuration;
using Serilog;
using Serilog.Events;

namespace Shared.Hosting.Abstractions.Logging
{
    public interface ILoggingPolicy
    {
        bool IsDefined();
        void Configure(LoggerConfiguration logConfig, IConfiguration configuration, string applicationName);
    }
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
                logConfig.ReadFrom.Configuration(configuration, Section);
                return;
            }

            var path = Path.Replace("{{{ApplicationName}}}", applicationName);

            logConfig
                .MinimumLevel.Is(MinimumLevel.Default)
                .WriteTo.Async(a =>
                {
                    a.Console(outputTemplate: OutputTemplate);
                    a.RollingFile(pathFormat: path, shared: true, outputTemplate: OutputTemplate);
                });

            foreach (var level in MinimumLevel.Override)
            {
                logConfig.MinimumLevel.Override(level.Key, level.Value);
            }
        }
    }
}