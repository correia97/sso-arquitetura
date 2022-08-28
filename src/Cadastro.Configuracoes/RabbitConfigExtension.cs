using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using RabbitMQ.Client;
using System.Diagnostics;

namespace Cadastro.Configuracoes
{

    public static class RabbitConfigExtension
    {
        public static IServiceCollection AddRabbitCustomConfiguration(this IServiceCollection services, IConfiguration configuration)
        {
            try
            {
                var factory = new ConnectionFactory
                {
                    Uri = new Uri(configuration.GetValue<string>("rabbit"))
                };
                IConnection connection = factory.CreateConnection();
                SetupRabbitMQ(connection);

                services.AddSingleton(connection);

            }
            catch (Exception ex)
            {
                Debug.WriteLine(ex.Message);
                throw;
            }
            return services;
        }

        private static void SetupRabbitMQ(IConnection connection)
        {
            try
            {
                using IModel model = connection.CreateModel();
                var cadastrarResult = model.QueueDeclare("cadastrar", true, false, false);
                Debug.WriteLine($"Queue {cadastrarResult.QueueName} message count {cadastrarResult.MessageCount}");

                var atualizarrResult = model.QueueDeclare("atualizar", true, false, false);
                Debug.WriteLine($"Queue {atualizarrResult.QueueName} message count {atualizarrResult.MessageCount}");

                var notificarResult = model.QueueDeclare("notificar", true, false, false);
                Debug.WriteLine($"Queue {notificarResult.QueueName} message count {notificarResult.MessageCount}");

                model.ExchangeDeclare("cadastro", ExchangeType.Topic, true);
                model.ExchangeDeclare("evento", ExchangeType.Fanout, true);
                model.QueueBind("cadastrar", "cadastro", "cadastrar");
                model.QueueBind("atualizar", "cadastro", "atualizar");
                model.QueueBind("notificar", "evento", "");
            }
            catch (Exception ex)
            {
                Debug.WriteLine(ex.Message);
                throw;
            }
        }
    }
}