using System;

namespace Infrastructure.Messages.Events
{
    [Serializable]
    public class AuthInitiationRequestEvent
    {
        public AuthInitiationRequestEvent(Guid eventId, string eventSource)
        {
            EventId = eventId;
            EventSource = eventSource;
        }

        public string EventSource { get; set; }
        public Guid EventId { get; set; }

        public override string ToString()
        {
            return string.Format("EventID:{1}, Event Source: {0} ",
                EventSource,
                EventId);
        }
    }
}
