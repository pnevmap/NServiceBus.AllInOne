using Microsoft.Extensions.Configuration;
using Serilog;

namespace Shared.Hosting.Abstractions.Logging
{
    public interface ILoggingPolicy
    {
        bool IsDefined();
        void Configure(LoggerConfiguration logConfig, IConfiguration configuration, string applicationName);
    }
}