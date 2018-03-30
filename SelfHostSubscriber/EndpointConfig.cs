
namespace SelfHostSubscriber
{
    using Infrastructure.Common;
    using NServiceBus;
    using NServiceBus.Features;

    /*
		This class configures this endpoint as a Server. More information about how to configure the NServiceBus host
		can be found here: http://particular.net/articles/the-nservicebus-host
	*/
    public class EndpointConfig : IConfigureThisEndpoint, AsA_Server,
                                  UsingTransport<RabbitMQTransport>,
                                  INeedInitialization
    {
        public void Customize(BusConfiguration busConfiguration)
        {
            busConfiguration.EndpointName("SelfHost.AuthEventConsumer.ConsoleWriter");

            // we are using In memory persistence for messages
            busConfiguration.UsePersistence<InMemoryPersistence>();


            // we are using JSON serialization
            busConfiguration.UseSerialization<JsonSerializer>();

            busConfiguration.DisableFeature<CriticalTimeMonitoring>();

            // we are using RabbitMQ as our messaging transport.  Other options are MSMQ
            var transport = busConfiguration.UseTransport<RabbitMQTransport>();

            transport.ConnectionString(Current.GetConnectionString());

            // NOTE: this is important and has to be set on publisher and subscriber endpoints.
            // We are using unobstrusive mode, so our messages/events do not have to implement IMessage, IEvent, etc
            var conventionsBuilder = busConfiguration.Conventions();
            conventionsBuilder.DefiningEventsAs(t => t.Namespace != null && t.Namespace == "Infrastructure.Messages.Events");

            busConfiguration.EnableInstallers();
        }
    }
}
