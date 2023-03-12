using Cadastro.Domain.Entities;
using Cadastro.Domain.Enums;
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
        public void SendEvent<T>(NotificationMessage<T> message)
        {
            IBasicProperties props = _model.CreateBasicProperties();
            props.ContentType = "text/json";
            props.DeliveryMode = 2;
            props.CorrelationId = message.CorrelationId.ToString();
            var messageBodyBytes = Encoding.UTF8.GetBytes(JsonSerializer.Serialize(message));
            _model.BasicPublish("evento", "notificar", props, messageBodyBytes);
            if (message.Success)
                _model.BasicPublish("evento", TipoNotificacao(message.Tipo), props, Encoding.UTF8.GetBytes(JsonSerializer.Serialize(message.Data)));
        }

        private static string TipoNotificacao(TipoEvento tipoEvento) => tipoEvento switch
        {
            TipoEvento.Cadastrado => "cadastrado",
            TipoEvento.Atualizado => "atualizado",
            TipoEvento.Desativado => "desativado",
            TipoEvento.Deletado => "deletado",
            _ => ""
        };
    }
}
