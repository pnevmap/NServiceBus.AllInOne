using System;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Infrastructure;

namespace Shared.DataAccess
{
    public static class DbContextOptionsBuilderExtensions
    {
        public static DbContextOptionsBuilder UseSqlServerDbSchema(this DbContextOptionsBuilder optionsBuilder, string schema)
        {
            if (optionsBuilder == null)
                throw new ArgumentNullException(nameof(optionsBuilder));

            if (string.IsNullOrWhiteSpace(schema))
                throw new ArgumentNullException(nameof(schema));

            var extension = optionsBuilder.Options.FindExtension<DbContextOptionsSchemaExtension>() ?? new DbContextOptionsSchemaExtension();

            extension.WithDbSchema(schema);

            ((IDbContextOptionsBuilderInfrastructure)optionsBuilder).AddOrUpdateExtension<DbContextOptionsSchemaExtension>(extension);

            return optionsBuilder;
        }

        public static DbContextOptionsBuilder<TContext> UseSqlServerDbSchema<TContext>(this DbContextOptionsBuilder<TContext> optionsBuilder, string schema) where TContext : DbContext
        {
            return (DbContextOptionsBuilder<TContext>)UseSqlServerDbSchema((DbContextOptionsBuilder)optionsBuilder, schema);
        }
    }
}

