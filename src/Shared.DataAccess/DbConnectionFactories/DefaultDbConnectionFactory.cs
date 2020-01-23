using System.Data.Common;
using System.Data.SqlClient;
using Microsoft.Extensions.Configuration;
using Serilog;
using Serilog.Events;

namespace Shared.DataAccess.DbConnectionFactories
{
    public class DefaultDbConnectionFactory : IDbConnectionFactory
    {
        private readonly ILogger _log;
        private readonly IConfiguration _configuration;

        public DefaultDbConnectionFactory(ILogger log, IConfiguration configuration)
        {
            _log = log.ForContext(GetType());
            _configuration = configuration;
        }
        
        public DbConnection CreateDbConnection(string name)
        {
            if(_log.IsEnabled(LogEventLevel.Verbose))
                _log.Verbose($"Creating DB connection {name}");
            
            return new SqlConnection(_configuration.GetConnectionString(name));
        }
    }
}