using System;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;
using Microsoft.Extensions.Configuration;
using Shared.Hosting.Abstractions;

namespace Shared.DataAccess
{
    public abstract class MigrationDbContextFactory<TContext> : IDesignTimeDbContextFactory<TContext> where TContext : DbContext
    {
        private readonly string _migrationsHistoryTable;
        private readonly string _connectionString;
        private readonly string _dbSchema;

        /// <summary>
        /// Creates context factory using default configuration from appsettings file. 
        /// </summary>
        /// <param name="migrationsHistoryTable">The migrations table name.</param>
        protected MigrationDbContextFactory(string migrationsHistoryTable = "__EFMigrationsHistory")
        {
            var configurationRoot = new ConfigurationBuilder().SetBasePath(AppContext.BaseDirectory)
                .AddJsonFile("appsettings.json")
                .AddJsonFile($"appsettings.{Environment.GetEnvironmentVariable("Hosting:Environment")}.json", true)
                .AddEnvironmentVariables()
                .Build();

            var defaults = Defaults.Read(configurationRoot);
            var connectionString = configurationRoot.GetConnectionString(defaults.DbConnectionString);

            _migrationsHistoryTable = migrationsHistoryTable;
            _connectionString = connectionString;
            _dbSchema = defaults.DbSchema;
        }

        /// <summary>
        /// Creates context factory using specified connection string and schema.
        /// </summary>
        /// <param name="connectionString">The connection string.</param>
        /// <param name="dbSchema">The database schema.</param>
        /// <param name="migrationsHistoryTable">The migrations table name.</param>
        protected MigrationDbContextFactory(string connectionString, string dbSchema = "dbo", string migrationsHistoryTable = "__EFMigrationsHistory")
        {
            _migrationsHistoryTable = migrationsHistoryTable;
            _connectionString = connectionString;
            _dbSchema = dbSchema;
        }

        public TContext CreateDbContext(string[] args)
        {
            return CreateDbContext(_connectionString, _dbSchema, _migrationsHistoryTable);
        }

        private static TContext CreateDbContext(string connectionString, string dbSchema, string migrationsHistoryTable)
        {
            if (string.IsNullOrEmpty(connectionString))
            {
                throw new ArgumentNullException(nameof(connectionString));
            }

            if (string.IsNullOrEmpty(dbSchema))
            {
                throw new ArgumentNullException(nameof(dbSchema));
            }

            if (string.IsNullOrEmpty(migrationsHistoryTable))
            {
                throw new ArgumentNullException(nameof(migrationsHistoryTable));
            }
            
            var dbContextOptions = new DbContextOptionsBuilder<TContext>()
                .UseSqlServer(connectionString, x => x.MigrationsHistoryTable(migrationsHistoryTable, dbSchema))
                .UseSqlServerDbSchema(dbSchema)
                .Options;

            return Activator.CreateInstance(typeof(TContext), dbContextOptions) as TContext;
        }
    }
}