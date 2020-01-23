using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using NServiceBus.Pipeline;
using Serilog;
using Serilog.Events;

namespace Shared.Messaging.Pipeline
{
    public class IncomingApplicationContextBehaviour : Behavior<ITransportReceiveContext>
    {
        private readonly ILogger _log;

        public IncomingApplicationContextBehaviour(ILogger log)
        {
            _log = log.ForContext(GetType());
        }

        public override async Task Invoke(ITransportReceiveContext context, Func<Task> next)
        {
            try
            {
                HydrateApplicationContext(context);

                await next();
            }
            finally
            {
            }
        }

        private void HydrateApplicationContext(ITransportReceiveContext context)
        {
            try
            {
                if (!IsOperationalMessage(context.Message.Headers)) 
                    return;
                

                if (!_log.IsEnabled(LogEventLevel.Verbose)) 
                    return;
                
                context.Message.Headers.TryGetValue("NServiceBus.EnclosedMessageTypes", out var messageTypes);
                
                _log.Verbose($"Hydrated application context (messageId={context.Message.MessageId}, messageTypes={messageTypes})");
            }
            catch (Exception e)
            {
                
                context.Message.Headers.TryGetValue("NServiceBus.EnclosedMessageTypes", out var messageTypes);
                
                _log.Error(e, $"Failed to hydrate the application context (messageId={context.Message.MessageId}, messageTypes={messageTypes})");
                
                throw;
            }
        }

        private static bool IsOperationalMessage(IReadOnlyDictionary<string, string> headers)
        {
            headers.TryGetValue("NServiceBus.MessageIntent", out var intent);

            if (string.IsNullOrWhiteSpace(intent))
                return true;

            switch (intent.ToLower())
            {
                case "subscribe":
                case "unsubscribe":
                    return false;
                default:
                    return true;
            }
        }
    }
}