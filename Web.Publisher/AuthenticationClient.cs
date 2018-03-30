using Infrastructure.Messages.Events;
using NServiceBus;
using System;
using System.Diagnostics;

namespace Web.Publisher
{
    public class AuthenticationClient
    {
        public IBus Bus { get; private set; }

        public AuthenticationClient(IBus bus)
        {
            Bus = bus;
        }

        public string Start()
        {
            var random = new Random(100);
            var processName = Process.GetCurrentProcess().ProcessName;
            var processId = Process.GetCurrentProcess().Id;

            var authEvent = new AuthInitiationRequestEvent(Guid.NewGuid(), String.Format("{0}:{1}", processId, processName));
            SendMessage(authEvent);

            return authEvent.ToString();
        }

        private void SendMessage<T>(T message)
        {
            Bus.Publish(message);
            Console.WriteLine(" [x] Sent {0}", message);
        }
    }
}