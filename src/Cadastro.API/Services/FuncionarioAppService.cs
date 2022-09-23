using Cadastro.API.Interfaces;
using Cadastro.API.Models.Response;
using Cadastro.Domain.Services;
using Domain.Entities;
using Microsoft.Extensions.Logging;
using Polly;
using RabbitMQ.Client;
using System;
using System.Collections.Generic;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;

namespace Cadastro.API.Services
{
    public class FuncionarioAppService : IFuncionarioAppService
    {
        private readonly IFuncionarioService _service;
        private readonly ILogger<FuncionarioAppService> _logger;
        private readonly AsyncPolicy _retryAsyncPolicy;
        private readonly IModel _model;
        public FuncionarioAppService(IModel model, IFuncionarioService service, ILogger<FuncionarioAppService> logger)
        {
            _service = service;
            _model = model;
            _logger = logger;
            _retryAsyncPolicy = Policy.Handle<Exception>()
                        .WaitAndRetryAsync(2, retryAttempt => TimeSpan.FromSeconds(Math.Pow(2, retryAttempt)),
                        (exception, timeSpan, retryCount, context) =>
                        {
                            // Add logic to be executed before each retry, such as logging
                            _logger.LogError(exception, $"Retry {retryCount} at: {DateTime.Now.ToString("dd/MM/yyyy HH:mm:ss")}");
                        });
        }

        public bool Cadastrar(Funcionario funcionario, Guid correlationId)
        {
            IBasicProperties props = _model.CreateBasicProperties();
            props.ContentType = "text/json";
            props.DeliveryMode = 2;
            props.CorrelationId = correlationId.ToString();
            var messageBodyBytes = Encoding.UTF8.GetBytes(JsonSerializer.Serialize(funcionario));
            _model.BasicPublish("cadastro", "cadastrar", props, messageBodyBytes);
            return true;
        }

        public bool Atualizar(Funcionario funcionario, Guid correlationId)
        {
            IBasicProperties props = _model.CreateBasicProperties();
            props.ContentType = "text/json";
            props.DeliveryMode = 2;
            props.CorrelationId = correlationId.ToString();
            var messageBodyBytes = Encoding.UTF8.GetBytes(JsonSerializer.Serialize(funcionario));
            _model.BasicPublish("cadastro", "atualizar", props, messageBodyBytes);
            return true;
        }

        public async Task<FuncionarioResponse> ObterPorId(Guid id)
        {
            var funcionario = await _retryAsyncPolicy.ExecuteAsync(async () =>
            {
                var funcionario = await _service.ObterPorId(id);
                if (funcionario == null)
                    return null;
                return funcionario;
            });
            return new FuncionarioResponse(funcionario);
        }

        public async Task<IEnumerable<FuncionarioResponse>> ObterTodos()
        {
            var funcionario = await _retryAsyncPolicy.ExecuteAsync(() => _service.ObterTodos());
            var result = new List<FuncionarioResponse>();
            foreach (var item in funcionario)
            {
                result.Add(new FuncionarioResponse(item));
            }
            return result;
        }
    }
}
