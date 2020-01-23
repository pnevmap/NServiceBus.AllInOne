using Microsoft.AspNetCore.Http;
using Serilog;

namespace Shared.Web.Bootstrapping.Middleware.Logging
{
    public class DevelopmentLoggingMiddleware : LoggingMiddleware
    {
        public DevelopmentLoggingMiddleware(RequestDelegate next, ILogger log) : base(next, log)
        {
            IsDevelopment = true;
        }
    }
}