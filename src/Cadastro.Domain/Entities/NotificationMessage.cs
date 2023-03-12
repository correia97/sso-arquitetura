using Cadastro.Domain.Enums;
using System;

namespace Cadastro.Domain.Entities
{
    public class NotificationMessage<T>
    {
        public NotificationMessage(Guid correlationId, Guid userId, TipoEvento tipo, bool success, T data)
        {
            CorrelationId = correlationId;
            UserId = userId;
            Tipo = tipo;
            Success = success;
            Data = data;
        }

        public Guid CorrelationId { get; private set; }
        public Guid UserId { get; private set; }
        public TipoEvento Tipo { get; private set; }
        public bool Success { get; private set; }

        public T Data { get; private set; }
    }
}
