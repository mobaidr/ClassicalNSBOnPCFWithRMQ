using Infrastructure.Messages.Events;
using NServiceBus;
using System;
using System.Diagnostics;
using System.Threading;

namespace Publisher
{
    public class AuthenticationClient
    {
        public IBus Bus { get; private set; }

        public AuthenticationClient(IBus bus)
        {
            Bus = bus;
        }

        public void Start()
        {
            var random = new Random(100);
            var processName = Process.GetCurrentProcess().ProcessName;
            var processId = Process.GetCurrentProcess().Id;
            //var exchange = "authentications";

            int count = 0;
            while (count++ < 2)
            {
                var stopWatch = new Stopwatch();
                stopWatch.Start();

                int numberOfMessages = 20;
                //2. create a message, that can be anything since it is a byte array
                for (var i = 0; i < numberOfMessages; i++)
                {
                    var orgId = random.Next(1000);
                    var authEvent = new AuthInitiationRequestEvent(Guid.NewGuid(), String.Format("{0}:{1}", processId, processName));
                    SendMessage(authEvent);
                    Thread.Sleep(2000);
                }

                stopWatch.Stop();

                Console.WriteLine("====================================================");
                Console.WriteLine($"[x] done sending {numberOfMessages} messages in " + stopWatch.ElapsedMilliseconds);
                Console.WriteLine("[x] Sending reset counter to consumers.");

                Console.ReadLine();
            }
        }

        private void SendMessage<T>(T message)
        {
            Bus.Publish(message);
            Console.WriteLine(" [x] Sent {0}", message);
        }

        public void Stop()
        {
            Console.WriteLine("Authentication Service ended");
        }
    }
}
