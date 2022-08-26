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
        public async Task<bool> Atualizar(Funcionario funcionario, string currentUserId)
        {
            using var span = _tracer.StartActiveSpan("Atualizar",SpanKind.Internal);
            var connection = _repositoryWrite.RecuperarConexao();
            var transaction = await _repositoryWrite.IniciarTransacao(connection);
            try
            {
                Funcionario baseFuncionario = await _repositoryRead.ObterPorId(connection, transaction, funcionario.Id);
                baseFuncionario.Atualizar(funcionario.Nome, funcionario.DataNascimento, funcionario.Email, funcionario.Matricula, funcionario.Cargo);
                baseFuncionario.AtualizarTelefones(funcionario.Telefones);
                baseFuncionario.AtualizarEnderecoComercial(funcionario.EnderecoComercial);
                baseFuncionario.AtualizarEnderecoResidencial(funcionario.EnderecoResidencial);
                var result = await _repositoryWrite.Atualizar(baseFuncionario, connection, transaction);

                if (!result)
                {
                    await _repositoryWrite.CancelarTransacao(connection, transaction);
                    return result;
                }
                if (funcionario.Telefones != null && funcionario.Telefones.Any())
                {
                    await TratarTelefones(funcionario.Telefones, connection, transaction);
                }

                if (funcionario.EnderecoResidencial != null && !string.IsNullOrEmpty(funcionario.EnderecoResidencial.Rua))
                {
                    await TratarEndereco(funcionario.EnderecoResidencial, connection, transaction);
                }

                if (funcionario.EnderecoComercial != null && !string.IsNullOrEmpty(funcionario.EnderecoComercial.Rua))
                {
                    await TratarEndereco(funcionario.EnderecoComercial, connection, transaction);
                }
                await _repositoryWrite.CompletarTransacao(connection, transaction);
                return result;
            }
            catch (Exception ex)
            {
                await _repositoryWrite.CancelarTransacao(connection, transaction);
                _logger.LogError(ex, "Erro ao atualizar");
                return false;
            }
            finally
            {
              await _repositoryWrite.DesalocarConexao(connection);
            }
        }

        public async Task<bool> Cadastrar(Funcionario funcionario)
        {
            using var span = _tracer.StartActiveSpan("Cadastrar", SpanKind.Internal);
            var connection = _repositoryWrite.RecuperarConexao();
            var transaction = await _repositoryWrite.IniciarTransacao(connection);

            try
            {
                Funcionario data = await _repositoryRead.ObterPorEmail(connection, transaction, funcionario.Email.EnderecoEmail);
                if (data != null)
                    return false;

                var result = await _repositoryWrite.Inserir(funcionario, connection, transaction);

                if (result == Guid.Empty)
                {
                    await _repositoryWrite.CancelarTransacao(connection, transaction);
                    return false;
                }
                if (funcionario.Telefones != null && funcionario.Telefones.Any())
                {
                    foreach (var item in funcionario.Telefones)
                        await _repositoryWrite.InserirTelefone(item, connection, transaction);
                }

                if (funcionario.EnderecoResidencial != null && !string.IsNullOrEmpty(funcionario.EnderecoResidencial.Rua))
                {
                    await _repositoryWrite.InserirEndereco(funcionario.EnderecoResidencial, connection, transaction);
                }

                if (funcionario.EnderecoComercial != null && !string.IsNullOrEmpty(funcionario.EnderecoComercial.Rua))
                {
                    await _repositoryWrite.InserirEndereco(funcionario.EnderecoComercial, connection, transaction);
                }
                await _repositoryWrite.CompletarTransacao(connection, transaction);

                return true;
            }
            catch (Exception ex)
            {
                await _repositoryWrite.CancelarTransacao(connection, transaction);
                _logger.LogError(ex, "Erro ao cadastrar");
                return false;
            }
            finally
            {
                await _repositoryWrite.DesalocarConexao(connection);
            }
        }

        public async Task<Funcionario> ObterPorId(Guid id)
        {

            using var span = _tracer.StartActiveSpan("ObterPorId", SpanKind.Internal);
            var connection = _repositoryRead.RecuperarConexao();
            try
            {
                Funcionario data = await _repositoryRead.ObterPorId(connection, null, id);
                return data;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Erro ao recuperar por ID");
                return null;
            }
            finally
            {
                await _repositoryRead.DesalocarConexao(connection);
            }
        }

        public async Task<IEnumerable<Funcionario>> ObterTodos()
        {

            using var span = _tracer.StartActiveSpan("ObterTodos", SpanKind.Internal);
            var connection = _repositoryRead.RecuperarConexao();
            try
            {
                IEnumerable<Funcionario> data = await _repositoryRead.ObterTodos(connection, null);
                return data;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Erro ao recuperar todos");
                return null;
            }
            finally
            {
                await _repositoryRead.DesalocarConexao(connection);
            }
        }

        private async Task TratarEndereco(Endereco endereco, IDbConnection dbConnection, IDbTransaction transaction)
        {

            using var span = _tracer.StartActiveSpan("TratarEndereco", SpanKind.Internal);
            if (endereco.Id > 0)
                await _repositoryWrite.AtualizarEndereco(endereco, dbConnection, transaction);
            else
                await _repositoryWrite.InserirEndereco(endereco, dbConnection, transaction);
        }

        private async Task TratarTelefones(IEnumerable<Telefone> telefones, IDbConnection dbConnection, IDbTransaction transaction)
        {

            using var span = _tracer.StartActiveSpan("TratarTelefones", SpanKind.Internal);
            foreach (var item in telefones)
            {
                if (item.Id > 0)
                    await _repositoryWrite.AtualizarTelefone(item, dbConnection, transaction);
                else
                    await _repositoryWrite.InserirTelefone(item, dbConnection, transaction);
            }
        }
    }
}
