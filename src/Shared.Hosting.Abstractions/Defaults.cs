using Microsoft.Extensions.Configuration;
using Shared.Hosting.Abstractions.Logging;

namespace Shared.Hosting.Abstractions
{
    public interface IDefaults
    {
        string DbConnectionString { get; set; }
        string DbSchema { get; set; }
        LoggingPolicy LoggingPolicy { get; set; }
    }

    public class Defaults : IDefaults
    {
        private const string SectionName = "Defaults";

        private const string DefaultDbSchema = "dbo";
        
        private string _dbConnectionString;
        private string _dbSchema;

        public string DbConnectionString
        {
            get => string.IsNullOrWhiteSpace(_dbConnectionString) ? "Db" : _dbConnectionString;
            set => _dbConnectionString = value;
        }
        public string DbSchema
        {
            get => string.IsNullOrWhiteSpace(_dbSchema) ? DefaultDbSchema : _dbSchema;
            set => _dbSchema = value;
        }
        
        public LoggingPolicy LoggingPolicy { get; set; } = new LoggingPolicy();

        public static Defaults Read(IConfiguration configuration)
        {
            return configuration.GetSection(SectionName)?.Get<Defaults>() ??
                   new Defaults();
        }
    }
}