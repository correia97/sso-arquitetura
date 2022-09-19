using System;

namespace Cadastro.Domain.Entities
{
    public class NotificationMessage
    {
        public NotificationMessage(Guid correlationId, Guid userId, string eventName, bool success)
        {
            CorrelationId = correlationId;
            UserId = userId;
            EventName = eventName;
            Success = success;
        }

        public Guid CorrelationId { get; private set; }
        public Guid UserId { get; private set; }
        public string EventName { get; private set; }
        public bool Success { get; private set; }
    }
}
