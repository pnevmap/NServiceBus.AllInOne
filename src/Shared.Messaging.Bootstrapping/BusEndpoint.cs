using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using NServiceBus;
using Serilog;

namespace Shared.Messaging
{
    public class BusEndpoint
    {
        private readonly string _name;
        private readonly ILogger _log;
        private readonly EndpointConfiguration _endpointConfiguration;
        private IEndpointInstance Endpoint { get; set; }

        public BusEndpoint(string name, EndpointConfiguration endpointConfiguration, ILogger log)
        {
            _name = name;
            _endpointConfiguration = endpointConfiguration;
            _log = log;
        }
        public async Task<IEndpointInstance> Start()
        {

            Endpoint = await NServiceBus.Endpoint.Start(_endpointConfiguration);

            _log.Information("Endpoint {endpointQueue} has started", _name);

            return Endpoint;
        }
        public Task Stop()
        {
            _log?.Information("Stopping {endpointQueue} _endpoint", _name);

            return Endpoint?.Stop();
        }


    }
}