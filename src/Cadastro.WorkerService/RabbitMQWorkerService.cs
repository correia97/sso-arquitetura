using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Polly;
using RabbitMQ.Client;
using RabbitMQ.Client.Events;
using System;
using System.Linq;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;

namespace Cadastro.WorkerService
{
    public abstract class RabbitMQWorkerService : BackgroundService
    {
        private readonly ILogger<Worker> _logger;
        private readonly IModel _model;
        private readonly IServiceProvider _serviceProvider;
        private readonly Policy _retryPolicy;
        private readonly AsyncPolicy _retryAsyncPolicy;
        public RabbitMQWorkerService(ILogger<Worker> logger, IModel model, IServiceProvider serviceProvider)
        {
            _logger = logger;
            _model = model;
            _serviceProvider = serviceProvider;
            _retryPolicy = Policy.Handle<TimeoutException>()
                        .Or<Exception>()
                        .WaitAndRetry(3, retryAttempt => TimeSpan.FromSeconds(Math.Pow(5, retryAttempt)),
                        (exception, timeSpan, retryCount, context) =>
                        {
                            // Add logic to be executed before each retry, such as logging
                            _logger.LogInformation("-------------------------------------------------------");
                            _logger.LogInformation("-------------------------------------------------------");
                            _logger.LogInformation("-------------------------------------------------------");
                            _logger.LogError(exception, "Retry {0} at: {1:dd/MM/yyyy HH:mm:ss}", retryCount, DateTime.Now);
                        });


            _retryAsyncPolicy = Policy.Handle<TimeoutException>()
                        .Or<Exception>()
                        .WaitAndRetryAsync(3, retryAttempt => TimeSpan.FromSeconds(Math.Pow(2, retryAttempt)),
                        (exception, timeSpan, retryCount, context) =>
                        {
                            // Add logic to be executed before each retry, such as logging
                            _logger.LogInformation("-------------------------------------------------------");
                            _logger.LogInformation("-------------------------------------------------------");
                            _logger.LogInformation("-------------------------------------------------------");
                            _logger.LogError(exception, "Retry {0} at: {1:dd/MM/yyyy HH:mm:ss}", retryCount, DateTime.Now);
                        });
        }
        protected void AddConsumer<TService, TMessage>(Func<TService, TMessage, Task> action, string queue)
        {
            _model.BasicQos(prefetchSize: 0, prefetchCount: 20, global: false);

            var consumer = new EventingBasicConsumer(_model);

            consumer.Received += async (sender, ea) => await ReceiveidMessage(ea, action, queue);

            _model.BasicConsume(queue, false, consumer);
        }

        protected async Task ReceiveidMessage<TService, TMessage>(BasicDeliverEventArgs ea, Func<TService, TMessage, Task> action, string queue)
        {
            _logger.LogInformation("Message received from {0} at: {1:dd/MM/yyyy HH:mm:ss}", queue, DateTime.Now);
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
                _logger.LogError(ex, "Consume {0} failed at: {1:dd/MM/yyyy HH:mm:ss}", queue, DateTime.Now);
                _model.BasicReject(ea.DeliveryTag, false);
            }

            if (canDispatch)
            {
                try
                {
                    _logger.LogInformation("Consume started at: {0:dd/MM/yyyy HH:mm:ss}", DateTime.Now);
                    await Dispatch(action, ea, messageObject);
                    _model.BasicAck(ea.DeliveryTag, false);
                    _logger.LogInformation("Consume {0} success at: {1:dd/MM/yyyy HH:mm:ss}", queue, DateTime.Now);
                }
                catch (NullReferenceException ex)
                {
                    _logger.LogError(ex, "Consume {0} failed at: {1:dd/MM/yyyy HH:mm:ss}", queue, DateTime.Now);
                    canDispatch = false;
                    _model.BasicNack(ea.DeliveryTag, false, dlqCount < 4);
                }
                catch (InvalidOperationException ex)
                {
                    _model.BasicReject(ea.DeliveryTag, false);
                    _logger.LogError(ex, "Consume {0} failed at: {1:dd/MM/yyyy HH:mm:ss}", queue, DateTime.Now);
                }
                catch (Exception ex)
                {
                    _model.BasicNack(ea.DeliveryTag, false, dlqCount < 4);
                    _logger.LogError(ex, "Consume {0} failed at: {1:dd/MM/yyyy HH:mm:ss}", queue, DateTime.Now);
                }
            }
        }

        private static TMessage GetMessageToDispatch<TMessage>(ReadOnlyMemory<byte> body)
            => JsonSerializer.Deserialize<TMessage>(Encoding.UTF8.GetString(body.ToArray()));

        private async Task Dispatch<TService, TMessage>(Func<TService, TMessage, Task> action, BasicDeliverEventArgs ea, TMessage messageObject)
        {
            using var scope = _serviceProvider.CreateAsyncScope();
            var service = scope.ServiceProvider.GetRequiredService<TService>();
            await _retryAsyncPolicy.ExecuteAsync(() => action.Invoke(service, messageObject));
        }

    }
}
