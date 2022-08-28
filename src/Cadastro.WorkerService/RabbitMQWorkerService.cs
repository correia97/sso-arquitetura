using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Polly;
using RabbitMQ.Client;
using RabbitMQ.Client.Events;
using System;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;

namespace Cadastro.WorkerService
{
    public abstract class RabbitMQWorkerService : BackgroundService
    {
        private readonly ILogger<Worker> _logger;
        private readonly IConnection _connection;
        private readonly IServiceProvider _serviceProvider;
        private readonly Policy _retryPolicy;
        public RabbitMQWorkerService(ILogger<Worker> logger, IConnection connection, IServiceProvider serviceProvider)
        {
            _logger = logger;
            _connection = connection;
            _serviceProvider = serviceProvider;
            _retryPolicy = Policy.Handle<TimeoutException>()
                        .Or<Exception>()
                        .WaitAndRetry(3, retryAttempt => TimeSpan.FromSeconds(Math.Pow(2, retryAttempt)),
                        (exception, timeSpan, retryCount, context) =>
                        {
                            // Add logic to be executed before each retry, such as logging

                            _logger.LogError(exception, "Retry {0} at: {1:dd/MM/yyyy HH:mm:ss}", retryCount, DateTimeOffset.Now);
                        });
        }
        protected EventingBasicConsumer AddConsumer<TService, TMessage>(Func<TService, TMessage, Task> action, string queue)
        {
            IModel model = _retryPolicy.Execute(() => this._connection.CreateModel());

            var consumer = new EventingBasicConsumer(model);

            consumer.Received += async (sender, ea) =>
            {
                _logger.LogInformation("Message received from {0} at: {1:dd/MM/yyyy HH:mm:ss}", queue, DateTimeOffset.Now);
                TMessage messageObject = default;
                bool? canDispatch = null;
                try
                {
                    messageObject = GetMessageToDispatch<TMessage>(ea.Body);
                    canDispatch = true;
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Consume {0} failed at: {1:dd/MM/yyyy HH:mm:ss}", queue, DateTimeOffset.Now);
                    canDispatch = false;
                    model.BasicReject(ea.DeliveryTag, false);
                }

                if (canDispatch == true)
                {
                    try
                    {
                        _logger.LogInformation("Consume started at: {0:dd/MM/yyyy HH:mm:ss}", DateTimeOffset.Now);
                        await Dispatch(action, ea, messageObject);
                        model.BasicAck(ea.DeliveryTag, false);
                        _logger.LogInformation("Consume {0} success at: {1:dd/MM/yyyy HH:mm:ss}", queue, DateTimeOffset.Now);

                    }
                    catch (InvalidOperationException ex)
                    {
                        model.BasicNack(ea.DeliveryTag, false, false);
                        _logger.LogError(ex, "Consume {0} failed at: {1:dd/MM/yyyy HH:mm:ss}", queue, DateTimeOffset.Now);
                    }
                    catch (Exception ex)
                    {
                        model.BasicNack(ea.DeliveryTag, false, true);
                        _logger.LogError(ex, "Consume {0} failed at: {1:dd/MM/yyyy HH:mm:ss}", queue, DateTimeOffset.Now);
                    }
                }
            };

            model.BasicConsume(queue, false, consumer);
            return consumer;
        }

        private static TMessage GetMessageToDispatch<TMessage>(ReadOnlyMemory<byte> body)
            => JsonSerializer.Deserialize<TMessage>(Encoding.UTF8.GetString(body.ToArray()));

        private async Task Dispatch<TService, TMessage>(Func<TService, TMessage, Task> action, BasicDeliverEventArgs ea, TMessage messageObject)
        {
            using (var scope = _serviceProvider.CreateAsyncScope())
            {
                var service = scope.ServiceProvider.GetRequiredService<TService>();

                await action.Invoke(service, messageObject);
            }
        }
    }
}
