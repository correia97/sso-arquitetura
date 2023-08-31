using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Polly;
using RabbitMQ.Client;
using RabbitMQ.Client.Events;
using System.Diagnostics;
using System.Diagnostics.CodeAnalysis;
using System.Text;
using System.Text.Json;

namespace Cadastro.Configuracoes
{
    [ExcludeFromCodeCoverage]
    public class RabbitMQConsumer
    {
        private IModel Model { get; set; }
        private readonly ILogger<RabbitMQConsumer> _logger;
        private readonly IServiceProvider _serviceProvider;
        private readonly AsyncPolicy _retryAsyncPolicy;
        private readonly ActivitySource _activity;
        private readonly IConfiguration _configuration;
        public RabbitMQConsumer(ILogger<RabbitMQConsumer> logger, ActivitySource activity, IModel model, IServiceProvider serviceProvider, IConfiguration configuration)
        {
            _logger = logger;
            Model = model;
            _serviceProvider = serviceProvider;
            _activity = activity;
            _configuration = configuration;
            _retryAsyncPolicy = Policy.Handle<TimeoutException>()
                        .Or<Exception>()
                        .WaitAndRetryAsync(3, retryAttempt => TimeSpan.FromSeconds(Math.Pow(2, retryAttempt)),
                        (exception, timeSpan, retryCount, context) =>
                        {
                            _logger.CustomLogError(exception, $"Retry {retryCount}");
                        });
        }
        public void AddConsumer<TService, TMessage>(Func<TService, TMessage, Task> action, string queue)
        {
            if (Model.IsClosed)
                Reconnect();

            Model.BasicQos(prefetchSize: 0, prefetchCount: 20, global: false);

            var consumer = new EventingBasicConsumer(Model);

            consumer.Received += async (sender, ea) => await ReceiveidMessage(ea, action, queue);

            Model.BasicConsume(queue, false, consumer);
        }

        private void Reconnect()
        {
            var connection = RabbitConfigExtension.CreateConnection(_configuration);
            Model = connection.CreateModel();
        }

        protected async Task ReceiveidMessage<TService, TMessage>(BasicDeliverEventArgs ea, Func<TService, TMessage, Task> action, string queue)
        {
            _logger.CustomLogInformation($"Message received from {queue} ");
            using var act = _activity.StartActivity("ReceiveidMessage");

            if (Model.IsClosed)
                Reconnect();

            TMessage messageObject = default;
            bool canDispatch = false;
            int dlqCount = 0;

            if (ea.BasicProperties.Headers != null &&
                ea.BasicProperties.Headers.Any(x => x.Key == "x-death"))
                dlqCount = (int)ea.BasicProperties.Headers.First(x => x.Key == "x-death").Value;

            try
            {
                messageObject = GetMessageToDispatch<TMessage>(ea.Body);
                canDispatch = true;
            }
            catch (Exception ex)
            {
                _logger.CustomLogError(ex, $"Consume {queue} failed ");
                Model.BasicReject(ea.DeliveryTag, false);
            }

            if (!canDispatch)
                return;

            try
            {
                _logger.CustomLogInformation($"Consume started ");
                await Dispatch(action, messageObject);
                Model.BasicAck(ea.DeliveryTag, false);
                _logger.CustomLogInformation($"Consume {queue} success ");
            }
            catch (NullReferenceException ex)
            {
                _logger.CustomLogError(ex, $"Consume {queue} failed ");
                Model.BasicNack(ea.DeliveryTag, false, dlqCount < 4);
            }
            catch (InvalidOperationException ex)
            {
                Model.BasicReject(ea.DeliveryTag, false);
                _logger.CustomLogError(ex, $"Consume {queue} failed ");
            }
            catch (Exception ex)
            {
                Model.BasicNack(ea.DeliveryTag, false, dlqCount < 4);
                _logger.CustomLogError(ex, $"Consume {queue} failed ");
            }
        }

        private static TMessage GetMessageToDispatch<TMessage>(ReadOnlyMemory<byte> body)
                                                                => JsonSerializer.Deserialize<TMessage>(Encoding.UTF8.GetString(body.ToArray()));

        private async Task Dispatch<TService, TMessage>(Func<TService, TMessage, Task> action, TMessage messageObject)
        {
            using var scope = _serviceProvider.CreateAsyncScope();
            var service = scope.ServiceProvider.GetRequiredService<TService>();
            await _retryAsyncPolicy.ExecuteAsync(() => action.Invoke(service, messageObject));
        }
    }
}
