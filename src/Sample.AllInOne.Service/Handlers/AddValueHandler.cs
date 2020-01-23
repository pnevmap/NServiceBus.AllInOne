using System.Threading.Tasks;
using NServiceBus;
using Sample.AllInOne.Service.Commands;
using Sample.AllInOne.Service.DataAccess;
using Shared.Messaging.UnitOfWork;

namespace Sample.AllInOne.Service.Handlers
{
    public class AddValueHandler : IHandleMessages<AddValue>
    {
        public Task Handle(AddValue message, IMessageHandlerContext context)
        {
            var repository = context.DataContext<ApplicationDataContext>();
            
            repository.AddValue(message.Value);

            return Task.CompletedTask;
        }
    }
}