using Cadastro.Domain.Entities;
using System.Threading.Tasks;

namespace Cadastro.Domain.Interfaces
{
    public interface INotificationService
    {
        void SendEvent(NotificationMessage message);
    }
}
