using System.Threading.Tasks;
using NServiceBus;
using Sample.AllInOne.Service.Commands;
using Sample.AllInOne.Service.DataAccess;
using Shared.Messaging;
using Shared.Messaging.UnitOfWork;

namespace Sample.AllInOne.Service.Handlers
{
    public class UpdateValueHandler : IHandleMessages<UpdateValue>
    {
        public async Task Handle(UpdateValue message, IMessageHandlerContext context)
        {
            var repository = context.DataContext<ApplicationDataContext>();

            var value = await repository.Get(message.Id);
            if(value == null)
                throw new UnrecoverableMessageException($"Cannot find value with id {message.Id} to update");

            value.Value = message.Value;
        }
    }
}