using System;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.Extensions.DependencyInjection;

namespace Shared.DataAccess
{
    public class DbContextOptionsSchemaExtension : IDbContextOptionsExtension
    {
        public string DbSchema { get; private set; }

        public virtual DbContextOptionsSchemaExtension WithDbSchema(string dbSchema)
        {
            if(string.IsNullOrWhiteSpace(dbSchema))
                throw new ArgumentNullException(nameof(dbSchema));

            DbSchema = dbSchema;
            
            return this;
        }

        public bool ApplyServices(IServiceCollection services)
        {
            return false;
        }

        public long GetServiceProviderHashCode()
        {
            return 0;
        }

        public void Validate(IDbContextOptions options)
        {
        }

        public string LogFragment => $"DbSchema={DbSchema}";
    }
}