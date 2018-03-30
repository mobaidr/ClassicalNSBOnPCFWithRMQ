using Infrastructure.Common;
using NServiceBus;
using System;
using System.Threading.Tasks;

namespace Subscribers
{
    class Program
    {
        private static IBus _bus = null;

        static void Main(string[] args)
        {
            MainAsync(args).GetAwaiter().GetResult();
        }

        static async Task MainAsync(string[] args)
        {
            Console.WriteLine("NSB Handler started..");
            AppDomain.CurrentDomain.ProcessExit += CurrentDomain_ProcessExit;

            var busConfiguration = new BusConfiguration();

            // this will be the name of this endpoint
            busConfiguration.EndpointName("AuthEventConsumer.ConsoleWriter");

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
            _bus = startableBus.Start();

            Console.WriteLine("Bus started..");
            await Task.Delay(-1);
        }

        private static void CurrentDomain_ProcessExit(object sender, EventArgs e)
        {
            _bus?.Dispose();
        }
    }
}
