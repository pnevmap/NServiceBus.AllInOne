using Microsoft.EntityFrameworkCore;
using NServiceBus;
using NServiceBus.ObjectBuilder;
using NServiceBus.Persistence;
using Serilog;
using Serilog.Events;

namespace Shared.Messaging.UnitOfWork
{
    public class EntityFrameworkUnitOfWork<TDbContext> where TDbContext : DbContext
    {
        private readonly IBuilder _builder;
        private readonly ILogger _log;
        private readonly string _schema;

        public EntityFrameworkUnitOfWork(IBuilder builder, string schema, ILogger log)
        {
            _builder = builder;
            _schema = schema;
            _log = log.ForContext(GetType());
        }

        public TDbContext GetDataContext(SynchronizedStorageSession storageSession)
        {
            var dbConnection = storageSession.SqlPersistenceSession().Connection;

            if (_log.IsEnabled(LogEventLevel.Verbose))
                _log.Verbose($"Creating {typeof(TDbContext).FullName} context");

            var dbContext = DbContextThreadAsyncFactory.CreateDbContext(dbConnection, _schema, _builder.Build<TDbContext>);

            // Use the same underlying ADO.NET transaction
            dbContext.Database.UseTransaction(storageSession.SqlPersistenceSession().Transaction);

            // Call SaveChanges before completing storage session
            storageSession.SqlPersistenceSession().OnSaveChanges(x =>
            {
                if (_log.IsEnabled(LogEventLevel.Verbose))
                    _log.Verbose($"Auto saving changes for {typeof(TDbContext).FullName} context");

                return dbContext.SaveChangesAsync();
            });

            return dbContext;
        }

    }
}
