using NServiceBus;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Infrastructure.Common
{
    public class NSBConfigure
    {
        public static BusConfiguration SetupWith(string endpointName, BusConfiguration configuration = null)
        {
            var busConfiguration = configuration ?? new BusConfiguration();

            // this will be the name of this endpoint
            busConfiguration.EndpointName(endpointName);

            // we are using JSON serialization
            busConfiguration.UseSerialization<JsonSerializer>();

            // we are using RabbitMQ as our messaging transport.  Other options are MSMQ
            var transport = busConfiguration.UseTransport<RabbitMQTransport>();

            transport.ConnectionString(Current.GetConnectionString());

            // we are using In memory persistence for messages
            busConfiguration.UsePersistence<InMemoryPersistence>();

            // NOTE: this is important and has to be set on publisher and subscriber endpoints.
            // We are using unobstrusive mode, so our messages/events do not have to implement IMessage, IEvent, etc
            var conventionsBuilder = busConfiguration.Conventions();
            conventionsBuilder.DefiningEventsAs(t => t.Namespace != null && t.Namespace == "Infrastructure.Messages.Events");

            busConfiguration.EnableInstallers();

            return configuration;
        }
    }
}
