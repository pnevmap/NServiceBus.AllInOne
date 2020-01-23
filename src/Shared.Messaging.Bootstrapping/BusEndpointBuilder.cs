using System;
using System.Linq;
using AutoMapper;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using NServiceBus;
using NServiceBus.Configuration.AdvancedExtensibility;
using NServiceBus.Logging;
using NServiceBus.Persistence.Sql;
using NServiceBus.Serilog;
using Serilog;
using Shared.DataAccess.DbConnectionFactories;
using Shared.Hosting.Abstractions;
using Shared.Hosting.Abstractions.Logging;
using Shared.Messaging.Pipeline;
using Shared.Messaging.UnitOfWork;
using SerilogLogger = Serilog.Core.Logger;

namespace Shared.Messaging
{
    public class BusEndpointBuilder<TDbContext> where TDbContext : DbContext
    {
        private IConfiguration Configuration { get; set; }
        private BusConfiguration BusConfiguration { get; set; }
        private Defaults Defaults { get; set; }
        private ILogger Log { get; set; }

        private EndpointConfiguration EndpointConfiguration { get; set; }
        public BusEndpointBuilder(IConfiguration configuration, BusConfiguration busConfiguration)
        {
            Defaults = Defaults.Read(configuration);
            Configuration = configuration;
            BusConfiguration = busConfiguration;
        }

        public BusEndpointBuilder<TDbContext> Configure()
        {

            if (Serilog.Log.Logger == SerilogLogger.None)
                Serilog.Log.Logger = Configuration.CreateNewLogger(BusConfiguration.Static.EndpointQueue);

            Log = Serilog.Log.Logger.ForContext(typeof(BusEndpointBuilder<TDbContext>));

            Log.Verbose("Initializing endpoint {endpointQueue}", BusConfiguration.Static.EndpointQueue);

            var endpointConfig = new EndpointConfiguration(BusConfiguration.Static.EndpointQueue);

            LogManager.Use<SerilogFactory>().WithLogger(Log);

            endpointConfig.EnableInstallers();

            RegisterPipelines(endpointConfig);

            endpointConfig.SendFailedMessagesTo(BusConfiguration.Static.ErrorQueue);

            var services = new ServiceCollection();

            RegisterComponents(services);

            BusConfiguration.Actions.ConfigureConventions?.Invoke(endpointConfig.Conventions());

            endpointConfig.UseSerialization<NewtonsoftSerializer>();

            ConfigureTransport(endpointConfig, BusConfiguration.Actions.ConfigureRouting);

            ConfigurePersistence(endpointConfig, services);

            BusConfiguration.Actions.CustomEndpointConfiguration?.Invoke(endpointConfig, services);

            endpointConfig.UseContainer<ServicesBuilder>(c =>
            {
                c.ExistingServices(services);
            });

            endpointConfig.Recoverability().AddUnrecoverableException<UnrecoverableMessageException>();

            Log.Information("Endpoint {endpointQueue} is initialized", BusConfiguration.Static.EndpointQueue);

            EndpointConfiguration = endpointConfig;
            return this;
        }

        public BusEndpoint BuildBusEndpoint()
        {
            return new BusEndpoint(BusConfiguration.Static.EndpointQueue, EndpointConfiguration, Log);
        }

        private void ConfigureTransport(EndpointConfiguration endpointConfiguration, Action<RoutingSettings<RabbitMQTransport>> configureRouting)
        {
            var busConfigurationStatic = Configuration.GetSection("BusOptions")?.Get<BusConfigurationStatic>();

            var transport = endpointConfiguration.UseTransport<RabbitMQTransport>();

            transport.UseConventionalRoutingTopology();

            transport.ConnectionString(busConfigurationStatic.RabbitMq.ConnectionString);

            var routing = transport.Routing();

            configureRouting?.Invoke(routing);
        }
        private void ConfigurePersistence(EndpointConfiguration endpointConfig, ServiceCollection services)
        {
            var persistence = endpointConfig.UsePersistence<SqlPersistence>();
            //persistence.DisableInstaller();
            var dialect = persistence.SqlDialect<SqlDialect.MsSqlServer>();
            dialect.Schema(Defaults.DbSchema);

            endpointConfig.EnableOutbox();

            var dbConnectionProvider = new DefaultDbConnectionFactory(Log, Configuration);

            endpointConfig.RegisterComponents(c => c.RegisterSingleton(typeof(IDbConnectionFactory), dbConnectionProvider));

            persistence.ConnectionBuilder(() => dbConnectionProvider.CreateDbConnection(Defaults.DbConnectionString));

            endpointConfig.Pipeline.Register(
                new UnitOfWorkSetupBehavior<TDbContext>(Defaults.DbSchema, Log),
                $"Sets up unit of work for the {typeof(TDbContext).FullName}");

            services.AddDbContext<TDbContext>(DbContextThreadAsyncFactory.ApplyCurrentDbConnection);
        }
        private void RegisterPipelines(EndpointConfiguration endpointConfig)
        {
            endpointConfig.Pipeline
                .Register(new IncomingApplicationContextBehaviour(Log), "Hydrates application context for incoming message");

            endpointConfig.Pipeline
                .Register(new LoggingBehavior(Log), "Default logging");
        }
        private void RegisterComponents(IServiceCollection services)
        {
            services.AddSingleton(Configuration);
            services.AddSingleton(Defaults);

            services.AddSingleton(Serilog.Log.Logger);

            if (BusConfiguration.Actions.AutomapperProfilesAssemblies != null && BusConfiguration.Actions.AutomapperProfilesAssemblies.Any())
            {
                services.AddAutoMapper(BusConfiguration.Actions.AutomapperProfilesAssemblies);
            }

            BusConfiguration.Actions.ConfigureServices?.Invoke(services);
        }
    }

}
