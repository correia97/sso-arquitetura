using Cadastro.Domain.Entities;

namespace Cadastro.Domain.Interfaces
{
    public interface INotificationService
    {
        void SendEvent<T>(NotificationMessage<T> message);
    }
}
