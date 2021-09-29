using Cadastro.API.Interfaces;
using Cadastro.Domain.Interfaces;
using Domain.Entities;
using RabbitMQ.Client;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Newtonsoft.Json;
using System.Text.Encodings;

namespace Cadastro.API.Services
{
    public  class FuncionarioAppService : IFuncionarioAppService
    {
        private readonly IConnection _connection;
        private readonly IFuncionarioReadRepository _repository;
        private readonly IModel _channel;
        public FuncionarioAppService(IConnection connection, IFuncionarioReadRepository repository)
        {
            _repository = repository;
            _connection = connection;
            _channel = _connection.CreateModel();
        }

        public bool Cadastrar(Funcionario funcionario)
        {
            try
            {
                IBasicProperties props = _channel.CreateBasicProperties();
                props.ContentType = "text/json";
                props.DeliveryMode = 2;
                var messageBodyBytes = System.Text.Encoding.UTF8.GetBytes(JsonConvert.SerializeObject(funcionario));
                _channel.BasicPublish("cadastro", "cadastro", props, messageBodyBytes);
                return true;
            }
            catch (Exception ex)
            {
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
                var messageBodyBytes = System.Text.Encoding.UTF8.GetBytes(JsonConvert.SerializeObject(funcionario));
                _channel.BasicPublish("cadastro", "atualizar", props, messageBodyBytes);
                return true;
            }
            catch (Exception ex)
            {
                return false;
            }
        }

        public async Task<Funcionario> ObterPorId(Guid id)
        {
            throw new NotImplementedException();
        }

        public async Task<IEnumerable<Funcionario>> ObterTodos()
        {
            throw new NotImplementedException();
        }
    }
}
