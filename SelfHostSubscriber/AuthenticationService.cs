using Infrastructure.Messages.Events;
using NServiceBus;
using System;

namespace SelfHostSubscriber
{
    public class AuthenticationService : IHandleMessages<AuthInitiationRequestEvent>
    {
        public void Handle(AuthInitiationRequestEvent message)
        {
            Console.WriteLine(message);
        }
    }
}
