using Cadastro.Domain.Interfaces;
using Domain.Entities;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using OpenTelemetry.Trace;
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
        private readonly Tracer _tracer;
        private IServiceProvider _serviceProvider;

        public Worker(ILogger<Worker> logger, Tracer tracer, IConnection connection, IServiceProvider serviceProvider)
        {
            _logger = logger;
            _tracer = tracer;
            _connection = connection;
            _serviceProvider = serviceProvider;
        }

        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            _logger.LogInformation("Worker running at: {0:dd/MM/yyyy HH:mm:ss}", DateTimeOffset.Now);

            IModel model = _connection.CreateModel();
            EventingBasicConsumer consumerCadastrar = this.BuildConsumer(model, Cadastrar);
            model.BasicConsume("cadastrar", false, consumerCadastrar);

            EventingBasicConsumer consumerAtualizar = this.BuildConsumer(model, Atualizar);
            model.BasicConsume("atualizar", false, consumerAtualizar);

            await Task.Delay(1000, stoppingToken);
        }

        private EventingBasicConsumer BuildConsumer(IModel model, Func<IFuncionarioService, string, IModel, ulong, Task> action)
        {
            var consumer = new EventingBasicConsumer(model);

            consumer.Received += async (sender, ea) =>
            {
                using (var scope = _serviceProvider.CreateAsyncScope())
                {
                    var funcionarioService = scope.ServiceProvider.GetRequiredService<IFuncionarioService>();

                    _logger.LogInformation("Message received at: {0:dd/MM/yyyy HH:mm:ss}", DateTimeOffset.Now);

                    var body = ea.Body.ToArray();

                    var message = Encoding.UTF8.GetString(body);

                    await action.Invoke(funcionarioService, message, model, ea.DeliveryTag);
                }
            };

            return consumer;
        }

        public async Task Cadastrar(IFuncionarioService _funcionarioService, string message, IModel model, ulong deliveryTag)
        {
            using var span = _tracer.StartRootSpan("Cadastrar", SpanKind.Consumer);
            _logger.LogInformation("Cadastrar started at: {0:dd/MM/yyyy HH:mm:ss}", DateTimeOffset.Now);
            try
            {
                var funcionario = JsonSerializer.Deserialize<Funcionario>(message);

                var result = await _funcionarioService.Cadastrar(funcionario);

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
                _logger.LogError(ex, "Cadastrar failed at: {0:dd/MM/yyyy HH:mm:ss}", DateTimeOffset.Now);
            }
        }

        public async Task Atualizar(IFuncionarioService _funcionarioService, string message, IModel model, ulong deliveryTag)
        {
            using var span = _tracer.StartRootSpan("Atualizar", SpanKind.Consumer);
            _logger.LogInformation("Atualizar started at: {0:dd/MM/yyyy HH:mm:ss}", DateTimeOffset.Now);

            try
            {
                var funcionario = JsonSerializer.Deserialize<Funcionario>(message);

                var success = await _funcionarioService.Atualizar(funcionario, "");
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
