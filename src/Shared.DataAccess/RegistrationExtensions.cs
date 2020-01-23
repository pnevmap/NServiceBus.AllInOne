using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Shared.DataAccess.DbConnectionFactories;
using Shared.Hosting.Abstractions;

namespace Shared.DataAccess
{
    public static class RegistrationExtensions
    {
        public static IServiceCollection AddSqlDbContext<TContext>(this IServiceCollection services, IDefaults defaults) where TContext : DbContext
        {
            services.TryAddSingleton<IDbConnectionFactory, DefaultDbConnectionFactory>();

            services.AddDbContext<TContext>((sp, o) =>
            {
                o.UseSqlServer(sp.GetRequiredService<IDbConnectionFactory>().CreateDbConnection(defaults.DbConnectionString));

                if (!string.IsNullOrWhiteSpace(defaults.DbSchema))
                    o.UseSqlServerDbSchema(defaults.DbSchema);
            });

            return services;
        }
    }
}