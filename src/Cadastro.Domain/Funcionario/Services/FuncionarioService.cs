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
        private readonly IFuncionarioReadRepository _repositoryRead;
        private readonly IFuncionarioWriteRepository _repositoryWrite;
        private readonly ILogger<FuncionarioService> _logger;
        public FuncionarioService(IFuncionarioReadRepository repositoryRead, IFuncionarioWriteRepository repositoryWrite, ILogger<FuncionarioService> logger)
        {
            _repositoryRead = repositoryRead;
            _repositoryWrite = repositoryWrite;
            _logger = logger;
        }
        public async Task<bool> Atualizar(Funcionario funcionario, string currentUserId)
        {
            try
            {
                Funcionario baseFuncionario = await _repositoryRead.ObterPorId(funcionario.Id);
                baseFuncionario.Atualizar(funcionario.Nome, funcionario.DataNascimento, funcionario.Email, funcionario.Matricula, funcionario.Cargo);
                baseFuncionario.AtualizarTelefones(funcionario.Telefones);
                baseFuncionario.AtualizarEnderecoComercial(funcionario.EnderecoComercial);
                baseFuncionario.AtualizarEnderecoResidencial(funcionario.EnderecoResidencial);
                var result = await _repositoryWrite.Atualizar(funcionario);

                return result;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Erro ao atualizar");
                return false;
            }
        }

        public async Task<bool> Cadastrar(Funcionario funcionario)
        {
            try
            {
                Funcionario data = await _repositoryRead.ObterPorEmail(funcionario.Email.EnderecoEmail);
                if (data != null)
                    return false;

                var result = await _repositoryWrite.Inserir(funcionario);

                return result != Guid.Empty;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Erro ao cadastrar");
                return false;
            }
        }

        public async Task<Funcionario> ObterPorId(Guid id)
        {
            try
            {
                Funcionario data = await _repositoryRead.ObterPorId(id);
                return data;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Erro ao recuperar por ID");
                return null;
            }
        }

        public async Task<IEnumerable<Funcionario>> ObterTodos()
        {
            try
            {
                IEnumerable<Funcionario> data = await _repositoryRead.ObterTodos();
                return data;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Erro ao recuperar todos");
                return null;
            }
        }
    }
}
