using System;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using NServiceBus.Pipeline;
using Serilog;

namespace Shared.Messaging.UnitOfWork
{
    public class UnitOfWorkSetupBehavior<TDbContext> : Behavior<IIncomingLogicalMessageContext> where TDbContext : DbContext
    {
        private readonly string _schema;
        private readonly ILogger _log;
        public UnitOfWorkSetupBehavior(string schema, ILogger log)
        {
            _schema = schema;
            _log = log.ForContext(GetType());
        }

        public override async Task Invoke(IIncomingLogicalMessageContext context, Func<Task> next)
        {
            var uow = new EntityFrameworkUnitOfWork<TDbContext>(context.Builder, _schema, _log);
            context.Extensions.Set<EntityFrameworkUnitOfWork<TDbContext>>(uow);
            await next();
            context.Extensions.Remove<EntityFrameworkUnitOfWork<TDbContext>>();
        }
    }
}

