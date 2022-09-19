using Cadastro.Domain.Entities;
using Cadastro.Domain.Interfaces;
using RabbitMQ.Client;
using System.Text;
using System.Text.Json;

namespace Cadastro.Data.Services
{
    public class NotificationService : INotificationService
    {
        private readonly IModel _model;
        public NotificationService(IModel model)
        {
            _model = model;
        }
        public void SendEvent(NotificationMessage message)
        {
            IBasicProperties props = _model.CreateBasicProperties();
            props.ContentType = "text/json";
            props.DeliveryMode = 2;
            props.CorrelationId = message.CorrelationId.ToString();
            var messageBodyBytes = Encoding.UTF8.GetBytes(JsonSerializer.Serialize(message));
            _model.BasicPublish("evento", "notificar", props, messageBodyBytes);
        }
    }
}
