using Infrastructure.Common;
using NServiceBus;
using System.Web.Http;

namespace Web.Publisher.Controllers
{
    public class MessageController : ApiController
    {

        [HttpGet]
        public string Get()
        {
            try
            {
                var busConfiguration = new BusConfiguration();

                // this will be the name of this endpoint
                busConfiguration.EndpointName("Publishers.Authentications");

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
                var startableBus = Bus.Create(busConfiguration);
                string message = string.Empty;

                using (var bus = startableBus.Start())
                {
                    message = new AuthenticationClient(bus).Start();
                }

                return message;
            }
            catch (System.Exception ex)
            {
                return ex.ToString();
            }
        }
    }
}