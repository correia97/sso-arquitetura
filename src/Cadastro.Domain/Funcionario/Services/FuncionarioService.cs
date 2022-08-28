using Cadastro.Domain.Interfaces;
using Domain.Entities;
using Microsoft.Extensions.Logging;
using OpenTelemetry.Trace;
using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Threading.Tasks;

namespace Cadastro.Domain.Services
{
    public class FuncionarioService : IFuncionarioService
    {
        private readonly IFuncionarioReadRepository _repositoryRead;
        private readonly IFuncionarioWriteRepository _repositoryWrite;
        private readonly ILogger<FuncionarioService> _logger;
        private readonly Tracer _tracer;
        public FuncionarioService(IFuncionarioReadRepository repositoryRead, IFuncionarioWriteRepository repositoryWrite, ILogger<FuncionarioService> logger, Tracer tracer)
        {
            _repositoryRead = repositoryRead;
            _repositoryWrite = repositoryWrite;
            _logger = logger;
            _tracer = tracer;
        }
        public async Task Atualizar(Funcionario funcionario)
        {
            using var span = _tracer.StartActiveSpan("Atualizar", SpanKind.Internal);
            var transaction = _repositoryWrite.IniciarTransacao();
            try
            {
                Funcionario baseFuncionario = await _repositoryRead.ObterPorId(transaction, funcionario.Id);
                baseFuncionario.Atualizar(funcionario.Nome, funcionario.DataNascimento, funcionario.Email, funcionario.Matricula, funcionario.Cargo);
                baseFuncionario.AtualizarTelefones(funcionario.Telefones);
                baseFuncionario.AtualizarEnderecoComercial(funcionario.EnderecoComercial);
                baseFuncionario.AtualizarEnderecoResidencial(funcionario.EnderecoResidencial);
                var result = await _repositoryWrite.Atualizar(baseFuncionario, transaction);

                if (!result)
                {
                    _repositoryWrite.CancelarTransacao(transaction);
                    // return result;
                }
                if (funcionario.Telefones != null && funcionario.Telefones.Any())
                {
                    await TratarTelefones(funcionario.Telefones, transaction);
                }

                if (funcionario.EnderecoResidencial != null && !string.IsNullOrEmpty(funcionario.EnderecoResidencial.Rua))
                {
                    await TratarEndereco(funcionario.EnderecoResidencial, transaction);
                }

                if (funcionario.EnderecoComercial != null && !string.IsNullOrEmpty(funcionario.EnderecoComercial.Rua))
                {
                    await TratarEndereco(funcionario.EnderecoComercial, transaction);
                }
                _repositoryWrite.CompletarTransacao(transaction);
                //return result;
            }
            catch (Exception ex)
            {
                _repositoryWrite.CancelarTransacao(transaction);
                _logger.LogError(ex, "Erro ao atualizar");
                //return false;
            }
        }

        public async Task Cadastrar(Funcionario funcionario)
        {
            using var span = _tracer.StartActiveSpan("Cadastrar", SpanKind.Internal);
            var transaction =  _repositoryWrite.IniciarTransacao();


            Funcionario data = await _repositoryRead.ObterPorEmail(transaction, funcionario.Email.EnderecoEmail);
            if (data != null)
                throw new InvalidOperationException("Funcionario já existe");

            var result = await _repositoryWrite.Inserir(funcionario, transaction);

            if (result == Guid.Empty)
            {
                _repositoryWrite.CancelarTransacao(transaction);
                throw new InvalidOperationException("Deu muito ruim!");
            }

            if (funcionario.Telefones != null && funcionario.Telefones.Any())
            {
                foreach (var item in funcionario.Telefones)
                    await _repositoryWrite.InserirTelefone(item, transaction);
            }

            if (funcionario.EnderecoResidencial != null && !string.IsNullOrEmpty(funcionario.EnderecoResidencial.Rua))
            {
                await _repositoryWrite.InserirEndereco(funcionario.EnderecoResidencial, transaction);
            }

            if (funcionario.EnderecoComercial != null && !string.IsNullOrEmpty(funcionario.EnderecoComercial.Rua))
            {
                await _repositoryWrite.InserirEndereco(funcionario.EnderecoComercial, transaction);
            }
            _repositoryWrite.CompletarTransacao(transaction);

        }

        public async Task<Funcionario> ObterPorId(Guid id)
        {
            using var span = _tracer.StartActiveSpan("ObterPorId", SpanKind.Internal);

            Funcionario data = await _repositoryRead.ObterPorId(null, id);
            return data;

        }

        public async Task<IEnumerable<Funcionario>> ObterTodos()
        {
            using var span = _tracer.StartActiveSpan("ObterTodos", SpanKind.Internal);
            IEnumerable<Funcionario> data = await _repositoryRead.ObterTodos(null);
            return data;

        }

        private async Task TratarEndereco(Endereco endereco, IDbTransaction transaction)
        {
            using var span = _tracer.StartActiveSpan("TratarEndereco", SpanKind.Internal);
            if (endereco.Id > 0)
                await _repositoryWrite.AtualizarEndereco(endereco, transaction);
            else
                await _repositoryWrite.InserirEndereco(endereco, transaction);
        }

        private async Task TratarTelefones(IEnumerable<Telefone> telefones, IDbTransaction transaction)
        {
            using var span = _tracer.StartActiveSpan("TratarTelefones", SpanKind.Internal);
            foreach (var item in telefones)
            {
                if (item.Id > 0)
                    await _repositoryWrite.AtualizarTelefone(item, transaction);
                else
                    await _repositoryWrite.InserirTelefone(item, transaction);
            }
        }
    }
}
