using System;
using System.Threading.Tasks;
using NServiceBus;

namespace NSBus.Message.Host
{
    public class PlaceOrderHandler : IHandleMessages<IBus6Message> 
    {
        public Task Handle(IBus6Message message, IMessageHandlerContext context)
        {
            Console.WriteLine("Message ID={0} Data={1}", message.ID, message.Data);
            return Task.FromResult(0);
        }
    }
}
