using Cadastro.Data.Repositories;
using Cadastro.Domain.Interfaces;
using Domain.Entities;
using Microsoft.Extensions.Configuration;
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
        private readonly IConfiguration _config;
        private readonly IFuncionarioWriteRepository _repository;

        public Worker(ILogger<Worker> logger, IConnection connection, IConfiguration config, IFuncionarioWriteRepository repository)
        {
            _logger = logger;
            _connection = connection;
            _config = config;
            _repository = repository;
        }

        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            _logger.LogInformation("Worker running at: {0:dd/MM/yyyy HH:mm:ss}", DateTimeOffset.Now);
            IModel model = _connection.CreateModel();
            EventingBasicConsumer consumer = await this.BuildConsumer(model,  Cadastrar);

            model.BasicConsume("cadastrar", false, consumer);

            await Task.Delay(1000, stoppingToken);
        }


        private async Task< EventingBasicConsumer> BuildConsumer(IModel model,  Action<string, IModel, ulong> action)
        {
            EventingBasicConsumer consumer = new EventingBasicConsumer(model);
            consumer.Received += async (sender, ea) =>
            {

                _logger.LogInformation("Message received at: {0:dd/MM/yyyy HH:mm:ss}", DateTimeOffset.Now);
                var body = ea.Body.ToArray();
                var message = Encoding.UTF8.GetString(body);

                 action.Invoke(message, model, ea.DeliveryTag);


            };
            return consumer;
        }

        public  void Cadastrar(string message, IModel model, ulong deliveryTag)
        {

            _logger.LogInformation("Cadastrar started at: {0:dd/MM/yyyy HH:mm:ss}", DateTimeOffset.Now);
            var funcionario = JsonSerializer.Deserialize<Funcionario>(message);

            var id =  _repository.Inserir(funcionario).Result;
            if (id != Guid.Empty)
            {
                model.BasicAck(deliveryTag, false);

                _logger.LogInformation("Cadastrar success at: {0:dd/MM/yyyy HH:mm:ss}", DateTimeOffset.Now);
            }
            else
            {
                model.BasicNack(deliveryTag, false, false);

                _logger.LogInformation("Cadastrar failed at: {0:dd/MM/yyyy HH:mm:ss}", DateTimeOffset.Now);
            }
        }


    }
}
