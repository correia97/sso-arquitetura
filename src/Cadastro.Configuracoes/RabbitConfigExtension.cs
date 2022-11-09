using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using RabbitMQ.Client;
using Serilog;
using System.Diagnostics.CodeAnalysis;

namespace Cadastro.Configuracoes
{
    [ExcludeFromCodeCoverage]
    public static class RabbitConfigExtension
    {
        public static IServiceCollection AddRabbitCustomConfiguration(this IServiceCollection services, IConfiguration configuration)
        {
            try
            {
                var connection = CreateConnection(configuration);
                var model = connection.CreateModel();
                SetupRabbitMQ(model);
                services.AddSingleton(model);
            }
            catch (Exception ex)
            {
                Log.Logger.Error(ex, "Erro ao Criar Conexão com RabbitMq");
                throw;
            }
            return services;
        }

        public static IConnection CreateConnection(IConfiguration configuration)
        {
            var factory = new ConnectionFactory
            {
                Uri = new Uri(configuration.GetValue<string>("rabbit"))
            };
            return factory.CreateConnection();
        }

        private static void SetupRabbitMQ(IModel model)
        {
            try
            {
                var cadastrarArgs = new Dictionary<string, object>();
                cadastrarArgs.Add("x-dead-letter-exchange", "cadastrar_deadletter");
                cadastrarArgs.Add("x-dead-letter-routing-key", "cadastrar_deadletter");
                model.QueueDeclare("cadastrar", true, false, false, cadastrarArgs);
                model.QueueDeclare("cadastrar_deadletter", true, false, false);

                var atualizarArgs = new Dictionary<string, object>();
                atualizarArgs.Add("x-dead-letter-exchange", "atualizar_deadletter");
                atualizarArgs.Add("x-dead-letter-routing-key", "atualizar_deadletter");
                model.QueueDeclare("atualizar_deadletter", true, false, false);
                model.QueueDeclare("atualizar", true, false, false, atualizarArgs);

                var removerArgs = new Dictionary<string, object>();
                removerArgs.Add("x-dead-letter-exchange", "remover_deadletter");
                removerArgs.Add("x-dead-letter-routing-key", "remover_deadletter");
                model.QueueDeclare("remover_deadletter", true, false, false);
                model.QueueDeclare("remover", true, false, false, removerArgs);

                var desativarArgs = new Dictionary<string, object>();
                desativarArgs.Add("x-dead-letter-exchange", "desativar_deadletter");
                desativarArgs.Add("x-dead-letter-routing-key", "desativar_deadletter");
                model.QueueDeclare("desativar_deadletter", true, false, false);
                model.QueueDeclare("desativar", true, false, false, desativarArgs);

                model.QueueDeclare("notificar", true, false, false);
                model.QueueDeclare("logs", true, false, false);
                model.ExchangeDeclare("cadastrar_deadletter", ExchangeType.Fanout, true, false, null);
                model.ExchangeDeclare("atualizar_deadletter", ExchangeType.Fanout, true, false, null);
                model.ExchangeDeclare("remover_deadletter", ExchangeType.Fanout, true, false, null);
                model.ExchangeDeclare("desativar_deadletter", ExchangeType.Fanout, true, false, null);
                model.ExchangeDeclare("cadastro", ExchangeType.Topic, true);
                model.ExchangeDeclare("evento", ExchangeType.Fanout, true);
                model.ExchangeDeclare("logs", ExchangeType.Fanout, true);

                model.QueueBind("cadastrar", "cadastro", "cadastrar");
                model.QueueBind("atualizar", "cadastro", "atualizar");
                model.QueueBind("remover", "cadastro", "remover");
                model.QueueBind("desativar", "cadastro", "desativar");
                model.QueueBind("cadastrar_deadletter", "cadastrar_deadletter", "");
                model.QueueBind("atualizar_deadletter", "atualizar_deadletter", "");
                model.QueueBind("remover_deadletter", "remover_deadletter", "");
                model.QueueBind("desativar_deadletter", "desativar_deadletter", "");
                model.QueueBind("notificar", "evento", "");
                model.QueueBind("logs", "logs", "");
            }
            catch (Exception ex)
            {
                Log.Logger.Error(ex, "Erro ao Criar filas");
                throw;
            }
        }
    }
}