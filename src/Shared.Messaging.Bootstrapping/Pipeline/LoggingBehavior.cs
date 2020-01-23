using System;
using System.Diagnostics;
using System.Threading.Tasks;
using Newtonsoft.Json;
using Newtonsoft.Json.Serialization;
using NServiceBus.Pipeline;
using Serilog;
using Serilog.Events;

namespace Shared.Messaging.Pipeline
{
    public class LoggingBehavior : Behavior<ITransportReceiveContext>
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
        
        private readonly ILogger _log;

        public LoggingBehavior(ILogger log)
        {
            _log = log.ForContext(GetType());
        }

        public override async Task Invoke(ITransportReceiveContext context, Func<Task> next)
        {
            var stopWatch = Stopwatch.StartNew();

            context.Message.Headers.TryGetValue("NServiceBus.EnclosedMessageTypes", out var messageTypes);
            context.Message.Headers.TryGetValue("NServiceBus.MessageId", out var messageId);
            
            if (_log.IsEnabled(LogEventLevel.Verbose))
                _log.Verbose($"Handling message (messageId={messageId}, messageTypes={messageTypes})");

            try
            {
                await next();

                stopWatch.Stop();

                _log.Information($"Handled message (messageId={messageId}, messageTypes={messageTypes}) in {stopWatch.ElapsedMilliseconds} milliseconds");
            }
            catch (Exception e)
            {
                stopWatch.Stop();

                _log.Error(e, $"Failed to complete message (messageId={messageId}, messageTypes={messageTypes}) handling, was executing for {stopWatch.ElapsedMilliseconds} milliseconds");

                throw;
            }
        }
    }
}