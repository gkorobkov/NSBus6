using NServiceBus;
using NServiceBus.Features;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using NServiceBus.Extensibility;
using NServiceBus.Persistence;
using NServiceBus.Settings;
using NServiceBus.Unicast.Subscriptions;
using NServiceBus.Unicast.Subscriptions.MessageDrivenSubscriptions;

namespace NSBus.Message.Client
{

    public class EndpointName
    {
        public const string MsmqTransportConfigInputQueue = "NServiceBus6.Test.Client.InputQueue";
        public const string MsmqTransportConfigAuditQueue = "NServiceBus6.Test.Client.AuditQueue";
        public const string MsmqTransportConfigErrorQueue = "NServiceBus6.Test.Client.ErrorQueue";
        public const string MsmqTransportConfigDestinationInputQueue = "NSBus.Message.Host.InputQueue";
    }
    
    class Program
    {
        static void Main(string[] args)
        {
            AsyncMain().GetAwaiter().GetResult(); 
        }

        static async Task AsyncMain()
        {
            Console.Title = "NSBus.Message.Client";

            var endpointConfiguration = new EndpointConfiguration(EndpointName.MsmqTransportConfigInputQueue);
            endpointConfiguration.EnableInstallers();
            var conventionsBuilder = endpointConfiguration.Conventions();
            conventionsBuilder.DefiningMessagesAs(t =>
                {
                    var res = t.FullName != null && t.FullName.Contains("Message") ||
                              t.Namespace != null && t.Namespace.Contains("Message");
                    if (res)
                    {
                        Console.WriteLine(t.FullName);
                    }
                    return res;
                }
            );
            endpointConfiguration.DisableFeature<TimeoutManager>();
            endpointConfiguration.UseTransport<MsmqTransport>();
            //endpointConfiguration.DisableFeature<MessageDrivenSubscriptions>();
            endpointConfiguration.UsePersistence<CustomPersistence>();
            endpointConfiguration.UseSerialization<XmlSerializer>();
            endpointConfiguration.DisableFeature<AutoSubscribe>();
            endpointConfiguration.DisableFeature<Sagas>();
            endpointConfiguration.SendFailedMessagesTo(EndpointName.MsmqTransportConfigErrorQueue);
            endpointConfiguration.AuditProcessedMessagesTo(EndpointName.MsmqTransportConfigAuditQueue);
            
            var endpointInstance = await Endpoint.Start(endpointConfiguration).ConfigureAwait(false);
            
            try
            {
                await SendOrder(endpointInstance);
            }
            finally
            {
                endpointInstance.Stop().ConfigureAwait(false);
            }
        }

        static async Task SendOrder(IEndpointInstance endpointInstance)
        {
            Console.WriteLine("Press enter to send a message");
            Console.WriteLine("Press any key to exit");

            while (true)
            {
                var key = Console.ReadKey();
                Console.WriteLine();
                if (key.Key != ConsoleKey.Enter)
                {
                    return;
                }
                var id = Guid.NewGuid().ToString();

                //// Publish
                try
                {
                    await endpointInstance.Publish<IBus6Message>(messageConstructor: message =>
                    {
                        message.ID = id;
                        message.Data = "Publish " + message.GetType();
                    });
                    Console.WriteLine("Publish message : {0} to {1}", id, EndpointName.MsmqTransportConfigDestinationInputQueue);

                }
                catch (Exception ex)
                {
                    Console.WriteLine("Exception in Publish message : {0} ", ex.Message);
                }
            }
        }

    }
    public class CustomPersistence : PersistenceDefinition
    {
        internal CustomPersistence()
        {
            Supports<StorageType.Subscriptions>(s => s.EnableFeatureByDefault<CustomFeature>());
        }
    }
    
    public class CustomFeature : Feature
    {
        internal CustomFeature()
        {
        }

        protected override void Setup(FeatureConfigurationContext context)
        {
            context.Container.ConfigureComponent(b => new CustomSubscriptionStorage(), DependencyLifecycle.SingleInstance);
        }
    }

    public class CustomSubscriptionStorage : ISubscriptionStorage
    {
        public Task Subscribe(Subscriber subscriber, MessageType messageType, ContextBag context)
        {
            return Task.FromResult(0);
        }

        public Task Unsubscribe(Subscriber subscriber, MessageType messageType, ContextBag context)
        {
            return Task.FromResult(0);
        }

        public Task<IEnumerable<Subscriber>> GetSubscriberAddressesForMessage(IEnumerable<MessageType> messageTypes, ContextBag context)
        {
            var result = new HashSet<Subscriber>
            {
                new Subscriber("UnknownQueueName", null),
                new Subscriber(EndpointName.MsmqTransportConfigDestinationInputQueue, null),
            };
            return Task.FromResult((IEnumerable<Subscriber>)result);
        }
    }
}
