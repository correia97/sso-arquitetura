using Cadastro.Domain.Interfaces;
using Domain.Entities;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Cadastro.Domain.Services
{
    public class FuncionarioService : IFuncionarioService
    {
        private readonly IFuncionarioRepository _repository;
        private readonly ILogger<FuncionarioService> _logger;
        public FuncionarioService(IFuncionarioRepository repository, ILogger<FuncionarioService> logger)
        {
            _repository = repository;
            _logger = logger;
        }
        public async Task<bool> Atualizar(Funcionario funcionario, string currentUserId)
        {
            var baseFuncionario = await _repository.RecuperarPorId(funcionario.Id);
            baseFuncionario.Atualizar(funcionario.Nome, funcionario.Email, funcionario.Matricula, funcionario.Cargo);
            baseFuncionario.AtualizarEnderecoComercial(funcionario.EnderecoComercial);
            baseFuncionario.AtualizarEnderecoResidencial(funcionario.EnderecoResidencial);
            throw new NotImplementedException();
        }

        public async Task<bool> Cadastrar(Funcionario funcionario)
        {
            var data = await _repository.BuscarPorEmail(funcionario.Email.EnderecoEmail);
            if (data != null)
                return false;

            var result = await _repository.Inserir(funcionario);

            return true;
        }

        public async Task<Funcionario> RecuperarPorId(Guid id)
        {
            var data = await _repository.RecuperarPorId(id);
            return data;
        }

        public async Task<IEnumerable<Funcionario>> RecuperarTodos()
        {
            var data = await _repository.RecuperarTodos();
            return data;
        }
    }
}
