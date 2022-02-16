using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using RabbitMQ.Client;


namespace Cadastro.Configuracoes
{

    public static class RabbitConfigExtension
    {
        public static IServiceCollection AddRabbitCustomConfiguration(this IServiceCollection services, IConfiguration configuration)
        {
            services.AddSingleton(sp =>
            {
                ConnectionFactory factory = new ConnectionFactory();
                factory.Uri = new System.Uri(configuration.GetValue<string>("rabbit"));
                IConnection connection = factory.CreateConnection();
                SetupRabbitMQ(connection);
                return connection;
            });

            return services;
        }

        private static void SetupRabbitMQ(IConnection connection)
        {
            try
            {
                IModel model = connection.CreateModel();
                var cadastrarResult = model.QueueDeclare("cadastrar", true, false, false);
                var atualizarrResult = model.QueueDeclare("atualizar", true, false, false);
                var notificarResult = model.QueueDeclare("notificar", true, false, false);
                model.ExchangeDeclare("cadastro", ExchangeType.Topic, true);

                model.ExchangeDeclare("evento", ExchangeType.Fanout, true);
                model.QueueBind("cadastrar", "cadastro", "cadastrar");
                model.QueueBind("atualizar", "cadastro", "atualizar");
                model.QueueBind("notificar", "evento", "");
            }
            catch (System.Exception ex)
            {
                Console.WriteLine(ex.Message);
                throw;
            }
        }
    }
}