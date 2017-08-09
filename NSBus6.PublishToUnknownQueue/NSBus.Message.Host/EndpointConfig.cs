
using System;
using System.Configuration;
using NServiceBus.Features;
using NServiceBus.Persistence.Legacy;

namespace NSBus.Message.Host
{
    using NServiceBus;

    public class EndpointConfig : IConfigureThisEndpoint
    {
        public void Customize(EndpointConfiguration endpointConfiguration)
        {
            Console.Title = "NSBus.Message.Host";

            endpointConfiguration.DefineEndpointName(ConfigurationManager.AppSettings["MsmqTransportConfig.InputQueue"]);
            endpointConfiguration.DisableFeature<TimeoutManager>();
            endpointConfiguration.UsePersistence<MsmqPersistence>();
            endpointConfiguration.UseSerialization<XmlSerializer>();
            endpointConfiguration.SendFailedMessagesTo(ConfigurationManager.AppSettings["MsmqTransportConfig.ErrorQueue"]);
            AssemblyScannerConfigurationExtensions.AssemblyScanner(endpointConfiguration);
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
        }
    }

}
