using Cadastro.API.Interfaces;
using Cadastro.Domain.Interfaces;
using Domain.Entities;
using Microsoft.Extensions.Logging;
using RabbitMQ.Client;
using System;
using System.Collections.Generic;
using System.Text.Json;
using System.Threading.Tasks;

namespace Cadastro.API.Services
{
    public class FuncionarioAppService : IFuncionarioAppService
    {
        private readonly IConnection _connection;
        private readonly IFuncionarioReadRepository _repository;
        private readonly IModel _channel;
        private readonly ILogger<FuncionarioAppService> _logger;
        public FuncionarioAppService(IConnection connection, IFuncionarioReadRepository repository, ILogger<FuncionarioAppService> logger)
        {
            _repository = repository;
            _connection = connection;
            _channel = _connection.CreateModel();
            _logger = logger;
        }

        public bool Cadastrar(Funcionario funcionario)
        {
            try
            {
                IBasicProperties props = _channel.CreateBasicProperties();
                props.ContentType = "text/json";
                props.DeliveryMode = 2;
                var messageBodyBytes = System.Text.Encoding.UTF8.GetBytes(JsonSerializer.Serialize(funcionario));
                _channel.BasicPublish("cadastro", "cadastrar", props, messageBodyBytes);
                return true;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Cadastrar");
                return false;
            }
        }
        public bool Atualizar(Funcionario funcionario, string currentUserId)
        {
            try
            {
                IBasicProperties props = _channel.CreateBasicProperties();
                props.ContentType = "text/json";
                props.DeliveryMode = 2;
                var messageBodyBytes = System.Text.Encoding.UTF8.GetBytes(JsonSerializer.Serialize(funcionario));
                _channel.BasicPublish("cadastro", "atualizar", props, messageBodyBytes);
                return true;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Atualizar");
                return false;
            }
        }

        public async Task<Funcionario> ObterPorId(Guid id)
        {
            try
            {
                var funcionario = await _repository.ObterPorId(id);
                return funcionario;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "ObterPorId");
                return null;
            }
        }

        public async Task<IEnumerable<Funcionario>> ObterTodos()
        {
            try
            {
                var funcionario = await _repository.ObterTodos();
                return funcionario;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "ObterTodos");
                return null;
            }
        }
    }
}
