using System;
using System.Data.Common;
using System.Threading;
using Microsoft.EntityFrameworkCore;
using Shared.DataAccess;

namespace Shared.Messaging.UnitOfWork
{
    internal static class DbContextThreadAsyncFactory
    {
        private static readonly AsyncLocal<ConnectionHolder> Current = new AsyncLocal<ConnectionHolder>();

        public static TDbContext CreateDbContext<TDbContext>(DbConnection dbConnection, string schema, Func<TDbContext> dbContextfactory) where TDbContext : DbContext
        {
            Current.Value = new ConnectionHolder { Connection = dbConnection, Schema = schema };

            try
            {
                return dbContextfactory();
            }
            finally
            {
                Current.Value.Connection = null;
                Current.Value.Schema = null;
                Current.Value = null;
            }
        }

        public static void ApplyCurrentDbConnection(DbContextOptionsBuilder dbContextOptionsBuilder)
        {
            if (Current.Value?.Connection == null)
                throw new ApplicationException("There is no current db connection");

            dbContextOptionsBuilder.UseSqlServer(Current.Value.Connection);

            if (!string.IsNullOrWhiteSpace(Current.Value.Schema))
                dbContextOptionsBuilder.UseSqlServerDbSchema(Current.Value.Schema);
        }

        private class ConnectionHolder
        {
            public DbConnection Connection;
            public string Schema;
        }
    }
}