using System;
using System.IO;
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

        protected MigrationDbContextFactory(string migrationsHistoryTable = "__EFMigrationsHistory")
        {
            var configurationRoot = new ConfigurationBuilder().SetBasePath(Directory.GetCurrentDirectory())
                .AddJsonFile("appsettings.json")
                .Build();

            var defaults = Defaults.Read(configurationRoot);
            var connectionString = configurationRoot.GetConnectionString(defaults.DbConnectionString);

            _migrationsHistoryTable = migrationsHistoryTable;
            _connectionString = connectionString;
            _dbSchema = defaults.DbSchema;
        }

        protected MigrationDbContextFactory(string connectionString, string dbSchema = "dbo", string migrationsHistoryTable = "__EFMigrationsHistory")
        {
            _migrationsHistoryTable = migrationsHistoryTable;
            _connectionString = connectionString;
            _dbSchema = dbSchema;
        }

        public TContext CreateDbContext(string[] args)
        {
            if (string.IsNullOrEmpty(_connectionString))
            {
                throw new ArgumentNullException(nameof(_connectionString));
            }

            if (string.IsNullOrEmpty(_dbSchema))
            {
                throw new ArgumentNullException(nameof(_dbSchema));
            }

            if (string.IsNullOrEmpty(_migrationsHistoryTable))
            {
                throw new ArgumentNullException(nameof(_migrationsHistoryTable));
            }
            
            var dbContextOptions = new DbContextOptionsBuilder<TContext>()
                .UseSqlServer(_connectionString, x => x.MigrationsHistoryTable(_migrationsHistoryTable, _dbSchema))
                .UseSqlServerDbSchema(_dbSchema)
                .Options;

            return Activator.CreateInstance(typeof(TContext), dbContextOptions) as TContext;
        }
    }
}