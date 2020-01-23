using System;
using System.Reflection;
using Microsoft.Extensions.DependencyInjection;
using NServiceBus;

namespace Shared.Messaging
{
    public class BusConfiguration
    {
        public static BusConfiguration CreateInstance(BusConfigurationStatic @static, BusConfigurationActions actions)
        {
            return new BusConfiguration
            {
                Static= @static,
                Actions = actions
            };
        }

        public BusConfigurationStatic Static { get; private set; }
        public BusConfigurationActions Actions { get; private set; }

    }
    public class RabbitMqDto
    {
        public string ConnectionString { get; set; }
    }
    public class BusConfigurationStatic
    {

        public string EndpointQueue { get; set; }
        public string ErrorQueue { get; set; } = "error";
        public RabbitMqDto RabbitMq { get; set; }
        //public dynamic SqlPersistence { get; set; }

    }
    public class BusConfigurationActions
    {
        public Assembly[] AutomapperProfilesAssemblies { get; set; }
        public Action<IServiceCollection> ConfigureServices { get; set; }
        public Action<RoutingSettings<RabbitMQTransport>> ConfigureRouting { get; set; }
        public Action<EndpointConfiguration, IServiceCollection> CustomEndpointConfiguration { get; set; }
        public Action<ConventionsBuilder> ConfigureConventions { get; set; } = builder =>
        {
            builder.DefiningCommandsAs(DefaultBusConventions.IsCommand);
            builder.DefiningEventsAs(DefaultBusConventions.IsEvent);
            builder.DefiningMessagesAs(DefaultBusConventions.IsMessage);
        };

    }
}