using Cadastro.Domain.Interfaces;
using Domain.Entities;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using RabbitMQ.Client;
using RabbitMQ.Client.Events;
using System;
using System.Text;
using System.Text.Json;
using System.Threading;
using System.Threading.Tasks;

namespace Cadastro.WorkerService
{
    public class Worker : BackgroundService
    {
        private readonly ILogger<Worker> _logger;
        private readonly IConnection _connection;
        private readonly IFuncionarioService _service;

        public Worker(ILogger<Worker> logger, IConnection connection, IFuncionarioService repository)
        {
            _logger = logger;
            _connection = connection;
            _service = repository;
        }

        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            _logger.LogInformation("Worker running at: {0:dd/MM/yyyy HH:mm:ss}", DateTimeOffset.Now);

            IModel model = _connection.CreateModel();

            EventingBasicConsumer consumerCadastrar = this.BuildConsumer(model, Cadastrar);

            EventingBasicConsumer consumerAtualizar = this.BuildConsumer(model, Atualizar);

            model.BasicConsume("cadastrar", false, consumerCadastrar);

            model.BasicConsume("atualizar", false, consumerAtualizar);

            await Task.Delay(1000, stoppingToken);
        }

        private EventingBasicConsumer BuildConsumer(IModel model, Func<string, IModel, ulong, Task> action)
        {
            var consumer = new EventingBasicConsumer(model);

            consumer.Received += async (sender, ea) =>
            {
                _logger.LogInformation("Message received at: {0:dd/MM/yyyy HH:mm:ss}", DateTimeOffset.Now);

                var body = ea.Body.ToArray();

                var message = Encoding.UTF8.GetString(body);

                await action.Invoke(message, model, ea.DeliveryTag);
            };

            return consumer;
        }

        public async Task Cadastrar(string message, IModel model, ulong deliveryTag)
        {
            _logger.LogInformation("Cadastrar started at: {0:dd/MM/yyyy HH:mm:ss}", DateTimeOffset.Now);

            try
            {
                var funcionario = JsonSerializer.Deserialize<Funcionario>(message);

                var result = await _service.Cadastrar(funcionario);

                if (result)
                {                    
                    model.BasicAck(deliveryTag, false);

                    _logger.LogInformation("Cadastrar success at: {0:dd/MM/yyyy HH:mm:ss}", DateTimeOffset.Now);
                }
                else
                {
                    model.BasicReject(deliveryTag, false);
                    _logger.LogInformation("Cadastrar failed at: {0:dd/MM/yyyy HH:mm:ss}", DateTimeOffset.Now);
                }
            }
            catch (Exception ex)
            {
                model.BasicNack(deliveryTag, false, true);
                _logger.LogError(ex, "Cadastrar failed at: {0:dd/MM/yyyy HH:mm:ss}  ex: {1}", DateTimeOffset.Now);
            }
        }

        public async Task Atualizar(string message, IModel model, ulong deliveryTag)
        {
            _logger.LogInformation("Atualizar started at: {0:dd/MM/yyyy HH:mm:ss}", DateTimeOffset.Now);

            try
            {
                var funcionario = JsonSerializer.Deserialize<Funcionario>(message);

                var success = await _service.Atualizar(funcionario, "");
                if (success)
                {
                    model.BasicAck(deliveryTag, false);
                    _logger.LogInformation("Atualizar success at: {0:dd/MM/yyyy HH:mm:ss}", DateTimeOffset.Now);
                }
                else
                {
                    model.BasicNack(deliveryTag, false, true);
                    _logger.LogInformation("Atualizar failed at: {0:dd/MM/yyyy HH:mm:ss}", DateTimeOffset.Now);
                }
            }
            catch (Exception ex)
            {
                model.BasicNack(deliveryTag, false, true);
                _logger.LogError(ex, "Atualizar failed at: {0:dd/MM/yyyy HH:mm:ss}", DateTimeOffset.Now);
            }
        }
    }
}
