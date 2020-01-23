using System;
using System.Diagnostics;
using System.Text;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Http.Extensions;
using Newtonsoft.Json;
using Newtonsoft.Json.Serialization;
using Serilog;

namespace Shared.Web.Bootstrapping.Middleware.Logging
{
    public class LoggingMiddleware
    {
        private static readonly JsonSerializerSettings SerializerSettings = new JsonSerializerSettings
        {
            Formatting = Formatting.None,
            NullValueHandling = NullValueHandling.Ignore,
            DateFormatHandling = DateFormatHandling.IsoDateFormat,
            ContractResolver = new DefaultContractResolver
            {
                NamingStrategy = new CamelCaseNamingStrategy()
            }
        };
        
        private readonly RequestDelegate _next;
        private readonly ILogger _log;
        
        protected bool IsDevelopment { get; set; }

        public LoggingMiddleware(RequestDelegate next, ILogger log)
        {
            _next = next;
            _log = log.ForContext(GetType());
        }

        public async Task Invoke(HttpContext context)
        {
            var displayUrl = context.Request.GetDisplayUrl();

            _log.Information(FormatEntryLogMessage(context.Request.Method, displayUrl));

            var stopWatch = new Stopwatch();

            stopWatch.Start();

            try
            {
                await _next.Invoke(context);

                stopWatch.Stop();

                var message = FormatExitLogMessage(MakeRequestName(context), displayUrl, stopWatch.ElapsedMilliseconds, context.Response.StatusCode);

                if (context.Response.StatusCode >= 200 && context.Response.StatusCode <= 399)
                    _log.Information(message);
                else
                    _log.Error(message);
            }
            catch (BadRequestException e)
            {
                stopWatch.Stop();
                
                if (context.Response.StatusCode != e.Error.Status)
                    context.Response.StatusCode = e.Error.Status;

                if (string.IsNullOrWhiteSpace(e.Error.TraceId))
                    e.Error.TraceId = context.TraceIdentifier;

                var details = JsonConvert.SerializeObject(e.Error, SerializerSettings);
                
                _log.Error(e,
                    FormatExceptionLogMessage(MakeRequestName(context), displayUrl, stopWatch.ElapsedMilliseconds, context.Response.StatusCode, details));

                await WriteJsonDetails(context, details);
            }
            catch (Exception e)
            {
                stopWatch.Stop();

                if (context.Response.StatusCode < 500)
                    context.Response.StatusCode = 500;

                _log.Error(e, FormatExceptionLogMessage(MakeRequestName(context), displayUrl, stopWatch.ElapsedMilliseconds, context.Response.StatusCode));

                await WriteExceptionResponse(context, e);
            }
        }

        private Task WriteExceptionResponse(HttpContext context, Exception e)
        {
            if (context.Response.HasStarted)
                return Task.CompletedTask;

            var responseMessage = IsDevelopment
                ? e.Message
                : $"{context.Response.StatusCode}";

            context.Response.ContentType = "text/plain; charset=utf-8";

            return context.Response.WriteAsync(responseMessage);
        }

        private static Task WriteJsonDetails(HttpContext context, string details)
        {
            context.Response.ContentType = "application/json; charset=utf-8";

            return context.Response.WriteAsync(details);
        }

        private static Task WriteTextDetails(HttpContext context, string details)
        {
            context.Response.ContentType = "text/plain; charset=utf-8";

            return context.Response.WriteAsync(details);
        }
        
        private static string MakeRequestName(HttpContext context)
        {
            var route = context.GetRouteInfo();

            return string.IsNullOrWhiteSpace(route.ControllerName) || string.IsNullOrWhiteSpace(route.ActionName)
                ? MakeRequestNameFromUrl(context.Request)
                : MakeRequestNameFromRoute(context.Request, route);
        }

        private static string MakeRequestNameFromUrl(HttpRequest request)
        {
            return new StringBuilder()
                .Append(request.Method)
                .Append(" ")
                .Append(request.Host.Value)
                .Append(request.PathBase.Value)
                .Append(request.Path.Value)
                .ToString();
        }

        private static string MakeRequestNameFromRoute(HttpRequest request, (string ControllerName, string ActionName) route)
        {
            return new StringBuilder()
                .Append(request.Method)
                .Append(" ")
                .Append(route.ControllerName)
                .Append(".")
                .Append(route.ActionName)
                .ToString();
        }

        private static string FormatEntryLogMessage(string requestMethod, string url)
        {
            return new StringBuilder()
                .Append("Starting Request ").Append(requestMethod).Append(" ").Append(url)
                .ToString();
        }

        private static string FormatExceptionLogMessage(string name, string url, long duration, int statusCode, string details = null)
        {
            var builder = new StringBuilder()
                .Append("Request ").Append(name);
                

            if (string.IsNullOrWhiteSpace(details))
            {
                builder
                    .Append(" completed in ").Append(duration).Append(" milliseconds with status code: ").Append(statusCode).Append(" ")
                    .Append(url)
                    .Append(" With exception:");
            }
            else
            {
                builder
                    .Append(" completed in ").Append(duration).Append(" milliseconds with status code & details: ").Append(statusCode).Append("; ").Append(details).Append(" ")
                    .Append(url)
                    .Append(" With exception:");
            }

            return builder.ToString();
        }

        private static string FormatExitLogMessage(string name, string url, long duration, int statusCode)
        {
            var builder = new StringBuilder()
                .Append("Request ").Append(name)
                .Append(" completed in ").Append(duration).Append(" milliseconds with status code: ").Append(statusCode).Append(" ")
                .Append(url);

            return builder.ToString();
        }
    }
}