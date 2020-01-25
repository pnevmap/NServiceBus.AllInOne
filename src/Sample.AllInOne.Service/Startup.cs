using System.Reflection;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using NServiceBus;
using Sample.AllInOne.Service.Commands;
using Sample.AllInOne.Service.DataAccess;
using Shared.DataAccess;
using Shared.Messaging;
using Shared.Web.Bootstrapping;

namespace Sample.AllInOne.Service
{
    public class Startup : WebHosStartup
    {

        public Startup(IConfiguration configuration)
            : base(configuration)
        {
        }

        protected override void ConfigureComponents(IServiceCollection services)
        {
            var busConfigurationStatic = Configuration.GetSection("BusOptions")?.Get<BusConfigurationStatic>();

            var busConfigurationActions = new BusConfigurationActions
            {
                ConfigureServices = _ => { },
                AutomapperProfilesAssemblies = new Assembly[0],
                ConfigureRouting = routing => routing.RouteToEndpoint(typeof(AddValue).Assembly, "NSB.AllInOne"),
                //ConfigureConventions = _ => { },
                CustomEndpointConfiguration = (c, s) => { }
            };

            var busConfiguration = BusConfiguration.CreateInstance(busConfigurationStatic, busConfigurationActions);

            var busEndpointBuilder = new BusEndpointBuilder<ApplicationDataContext>(Configuration, busConfiguration);

            var instance = busEndpointBuilder
                            .Configure()
                            .BuildBusEndpoint()
                            .Start()
                            .GetAwaiter()
                            .GetResult();

            services.AddSingleton<IMessageSession>(instance);

            services.AddSingleton<IDbConnectionFactory, DefaultDbConnectionFactory>();

            services.AddDbContext<ApplicationDataContext>((sp, o) =>
            {
                o.UseSqlServer(sp.GetRequiredService<IDbConnectionFactory>().CreateDbConnection(Defaults.DbConnectionString));

                if (!string.IsNullOrWhiteSpace(Defaults.DbSchema))
                    o.UseSqlServerDbSchema(Defaults.DbSchema);
            });
        }

        protected override void BuildMvc(IMvcBuilder mvcBuilder)
        {
            mvcBuilder.AddMvcOptions(option => option.EnableEndpointRouting = false);
            base.BuildMvc(mvcBuilder);
        }
        private static void ConfigureBusServices(IServiceCollection services)
        {
        }

    }
}