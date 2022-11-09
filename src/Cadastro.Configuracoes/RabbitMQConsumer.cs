﻿using Microsoft.Extensions.Configuration;
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
        private readonly ILogger<RabbitMQConsumer> _logger;
        private  IModel _model { get; set; }
        private readonly IServiceProvider _serviceProvider;
        private readonly AsyncPolicy _retryAsyncPolicy;
        private readonly ActivitySource _activity;
        private readonly IConfiguration _configuration;
        public RabbitMQConsumer(ILogger<RabbitMQConsumer> logger, ActivitySource activity, IModel model, IServiceProvider serviceProvider, IConfiguration configuration)
        {
            _logger = logger;
            _model = model;
            _serviceProvider = serviceProvider;
            _activity = activity;
            _configuration = configuration;
            _retryAsyncPolicy = Policy.Handle<TimeoutException>()
                        .Or<Exception>()
                        .WaitAndRetryAsync(3, retryAttempt => TimeSpan.FromSeconds(Math.Pow(2, retryAttempt)),
                        (exception, timeSpan, retryCount, context) =>
                        {
                            // Add logic to be executed before each retry, such as logging
                            _logger.LogInformation("-------------------------------------------------------");
                            _logger.LogInformation("-------------------------------------------------------");
                            _logger.LogInformation("-------------------------------------------------------");
                            _logger.LogError(exception, $"Retry {retryCount} at: {DateTime.Now.ToString("dd/MM/yyyy HH:mm:ss")}");
                        });
        }
        public void AddConsumer<TService, TMessage>(Func<TService, TMessage, Task> action, string queue)
        {
            if (_model.IsClosed)
                Reconnect();

            _model.BasicQos(prefetchSize: 0, prefetchCount: 20, global: false);

            var consumer = new EventingBasicConsumer(_model);

            consumer.Received += async (sender, ea) => await ReceiveidMessage(ea, action, queue);           

            _model.BasicConsume(queue, false, consumer);
        }

        private void Reconnect()
        {
            var connection = RabbitConfigExtension.CreateConnection(_configuration);
            _model = connection.CreateModel();
        }

        protected async Task ReceiveidMessage<TService, TMessage>(BasicDeliverEventArgs ea, Func<TService, TMessage, Task> action, string queue)
        {
            _logger.LogInformation($"Message received from {queue} at: {DateTime.Now.ToString("dd/MM/yyyy HH:mm:ss")}");
            using var act = _activity.StartActivity("ReceiveidMessage");
            
            if (_model.IsClosed)
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
                _logger.LogError(ex, $"Consume {queue} failed at: {DateTime.Now.ToString("dd/MM/yyyy HH:mm:ss")}");
                _model.BasicReject(ea.DeliveryTag, false);
            }

            if (!canDispatch)
                return;

            try
            {
                _logger.LogInformation($"Consume started at: {DateTime.Now.ToString("dd/MM/yyyy HH:mm:ss")}");
                await Dispatch(action, ea, messageObject);
                _model.BasicAck(ea.DeliveryTag, false);
                _logger.LogInformation($"Consume {queue} success at: {DateTime.Now.ToString("dd/MM/yyyy HH:mm:ss")}");
            }
            catch (NullReferenceException ex)
            {
                _logger.LogError(ex, $"Consume {queue} failed at: {DateTime.Now.ToString("dd/MM/yyyy HH:mm:ss")}");
                _model.BasicNack(ea.DeliveryTag, false, dlqCount < 4);
            }
            catch (InvalidOperationException ex)
            {
                _model.BasicReject(ea.DeliveryTag, false);
                _logger.LogError(ex, $"Consume {queue} failed at: {DateTime.Now.ToString("dd/MM/yyyy HH:mm:ss")}");
            }
            catch (Exception ex)
            {
                _model.BasicNack(ea.DeliveryTag, false, dlqCount < 4);
                _logger.LogError(ex, $"Consume {queue} failed at: {DateTime.Now.ToString("dd/MM/yyyy HH:mm:ss")}");
            }
        }

        private static TMessage GetMessageToDispatch<TMessage>(ReadOnlyMemory<byte> body)
                                                                => JsonSerializer.Deserialize<TMessage>(Encoding.UTF8.GetString(body.ToArray()));

        private async Task Dispatch<TService, TMessage>(Func<TService, TMessage, Task> action, BasicDeliverEventArgs ea, TMessage messageObject)
        {
            using (var scope = _serviceProvider.CreateAsyncScope())
            {
                var service = scope.ServiceProvider.GetRequiredService<TService>();
                await _retryAsyncPolicy.ExecuteAsync(() => action.Invoke(service, messageObject));
            }
        }
    }
}
