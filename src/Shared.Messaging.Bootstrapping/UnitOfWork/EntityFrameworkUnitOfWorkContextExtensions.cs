using Microsoft.EntityFrameworkCore;
using NServiceBus;

namespace Shared.Messaging.UnitOfWork
{
    public static class EntityFrameworkUnitOfWorkContextExtensions
    {
        public static TContext DataContext<TContext>(this IMessageHandlerContext context) where TContext : DbContext
        {
            var uow = context.Extensions.Get<EntityFrameworkUnitOfWork<TContext>>();
            return uow.GetDataContext(context.SynchronizedStorageSession);
        }

    }
}
