using Cadastro.Domain.Interfaces;
using Domain.Entities;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using RabbitMQ.Client;
using RabbitMQ.Client.Events;
using System;
using System.Linq;
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
        private readonly IFuncionarioWriteRepository _repository;

        public Worker(ILogger<Worker> logger, IConnection connection, IFuncionarioWriteRepository repository)
        {
            _logger = logger;
            _connection = connection;
            _repository = repository;
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

                var id = await _repository.Inserir(funcionario);

                if (id != Guid.Empty)
                {
                    if (funcionario.Telefones != null && funcionario.Telefones.Any())
                    {
                        foreach (var item in funcionario.Telefones)
                            await _repository.InserirTelefone(item);
                    }

                    if (funcionario.EnderecoResidencial != null && !string.IsNullOrEmpty(funcionario.EnderecoResidencial.Rua))
                    {
                        await _repository.InserirEndereco(funcionario.EnderecoResidencial);
                    }

                    if (funcionario.EnderecoComercial != null && !string.IsNullOrEmpty(funcionario.EnderecoComercial.Rua))
                    {
                        await _repository.InserirEndereco(funcionario.EnderecoComercial);
                    }

                    model.BasicAck(deliveryTag, false);

                    _logger.LogInformation("Cadastrar success at: {0:dd/MM/yyyy HH:mm:ss}", DateTimeOffset.Now);
                }
                else
                {
                    model.BasicNack(deliveryTag, false, false);

                    _logger.LogInformation("Cadastrar failed at: {0:dd/MM/yyyy HH:mm:ss}", DateTimeOffset.Now);
                }
            }
            catch (Exception ex)
            {
                model.BasicNack(deliveryTag, false, false);

                _logger.LogError(ex, "Cadastrar failed at: {0:dd/MM/yyyy HH:mm:ss}  ex: {1}", DateTimeOffset.Now);

                throw;
            }
        }

        public async Task Atualizar(string message, IModel model, ulong deliveryTag)
        {
            _logger.LogInformation("Atualizar started at: {0:dd/MM/yyyy HH:mm:ss}", DateTimeOffset.Now);

            try
            {
                var funcionario = JsonSerializer.Deserialize<Funcionario>(message);

                var success = await _repository.Atualizar(funcionario);
                if (success)
                {
                    if (funcionario.Telefones != null && funcionario.Telefones.Any())
                    {
                        foreach (var item in funcionario.Telefones)
                        {
                            if (item.Id > 0)
                                await _repository.AtualizarTelefone(item);
                            else
                                await _repository.InserirTelefone(item);
                        }
                    }

                    if (funcionario.EnderecoResidencial != null && !string.IsNullOrEmpty(funcionario.EnderecoResidencial.Rua))
                    {
                        if (funcionario.EnderecoResidencial.Id > 0)
                            await _repository.AtualizarEndereco(funcionario.EnderecoResidencial);
                        else
                            await _repository.InserirEndereco(funcionario.EnderecoResidencial);
                    }

                    if (funcionario.EnderecoComercial != null && !string.IsNullOrEmpty(funcionario.EnderecoComercial.Rua))
                    {
                        if (funcionario.EnderecoComercial.Id > 0)
                            await _repository.AtualizarEndereco(funcionario.EnderecoComercial);
                        else
                            await _repository.InserirEndereco(funcionario.EnderecoComercial);
                    }

                    model.BasicAck(deliveryTag, false);

                    _logger.LogInformation("Atualizar success at: {0:dd/MM/yyyy HH:mm:ss}", DateTimeOffset.Now);
                }
                else
                {
                    model.BasicNack(deliveryTag, false, false);

                    _logger.LogInformation("Atualizar failed at: {0:dd/MM/yyyy HH:mm:ss}", DateTimeOffset.Now);
                }
            }
            catch (Exception ex)
            {
                model.BasicNack(deliveryTag, false, false);

                _logger.LogError(ex, "Atualizar failed at: {0:dd/MM/yyyy HH:mm:ss}", DateTimeOffset.Now);

                throw;
            }
        }
    }
}
