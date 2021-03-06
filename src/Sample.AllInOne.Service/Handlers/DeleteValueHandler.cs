using System.Threading.Tasks;
using NServiceBus;
using Sample.AllInOne.Service.Commands;
using Sample.AllInOne.Service.DataAccess;
using Shared.Messaging;
using Shared.Messaging.UnitOfWork;

namespace Sample.AllInOne.Service.Handlers
{
    public class DeleteValueHandler : IHandleMessages<DeleteValue>
    {
        public async Task Handle(DeleteValue message, IMessageHandlerContext context)
        {

            var repository = context.DataContext<ApplicationDataContext>();

            var value = await repository.Get(message.Id);
            if (value == null)
                throw new UnrecoverableMessageException($"Cannot find value with id {message.Id} to delete");

            repository.DeleteValue(value);
        }
    }
}