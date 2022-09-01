using Cadastro.Domain.Enums;
using Cadastro.Domain.Interfaces;
using Domain.Entities;
using Microsoft.Extensions.Logging;
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
        public FuncionarioService(IFuncionarioReadRepository repositoryRead, IFuncionarioWriteRepository repositoryWrite, ILogger<FuncionarioService> logger)
        {
            _repositoryRead = repositoryRead;
            _repositoryWrite = repositoryWrite;
            _logger = logger;
        }
        public async Task Atualizar(Funcionario funcionario)
        {
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
                    _repositoryWrite.CancelarTransacao(transaction);

                if (funcionario.Telefones != null && funcionario.Telefones.Any())
                    await TratarTelefones(funcionario.Telefones, transaction);


                if (funcionario.EnderecoResidencial != null && !string.IsNullOrEmpty(funcionario.EnderecoResidencial.Rua))
                    await TratarEndereco(funcionario.EnderecoResidencial, transaction);


                if (funcionario.EnderecoComercial != null && !string.IsNullOrEmpty(funcionario.EnderecoComercial.Rua))
                    await TratarEndereco(funcionario.EnderecoComercial, transaction);

                _repositoryWrite.CompletarTransacao(transaction);
            }
            catch (Exception ex)
            {
                _repositoryWrite.CancelarTransacao(transaction);
                _logger.LogError(ex, "Erro ao atualizar");
                throw;
            }
        }

        public async Task Cadastrar(Funcionario funcionario)
        {
            var transaction = _repositoryWrite.IniciarTransacao();

            try
            {
                Funcionario data = await _repositoryRead.ObterPorEmail(transaction, funcionario.Email.EnderecoEmail);

                if (data != null)
                    throw new InvalidOperationException("Funcionario já existe");

                var result = await _repositoryWrite.Inserir(funcionario, transaction);

                if (result == Guid.Empty)                
                    throw new InvalidOperationException("Deu muito ruim!");                

                if (funcionario.Telefones != null && funcionario.Telefones.Any())
                {
                    foreach (var item in funcionario.Telefones)
                        await _repositoryWrite.InserirTelefone(item, transaction);
                }

                if (funcionario.EnderecoResidencial != null && !string.IsNullOrEmpty(funcionario.EnderecoResidencial.Rua))
                    await _repositoryWrite.InserirEndereco(funcionario.EnderecoResidencial, transaction);


                if (funcionario.EnderecoComercial != null && !string.IsNullOrEmpty(funcionario.EnderecoComercial.Rua))
                    await _repositoryWrite.InserirEndereco(funcionario.EnderecoComercial, transaction);

                _repositoryWrite.CompletarTransacao(transaction);
            }
            catch (Exception ex)
            {
                _repositoryWrite.CancelarTransacao(transaction);
                _logger.LogError(ex, "Erro ao Cadastrar");
                throw;
            }
        }

        public async Task<Funcionario> ObterPorId(Guid id)
        {
            Funcionario funcionario = await _repositoryRead.ObterPorId(null, id);


            var enderecos = await _repositoryRead.ObterEnderecosPorFuncionarioId(null, id);
            var telefones = await _repositoryRead.ObterTelefonesPorFuncionarioId(null, id);

            if (telefones != null && telefones.Any())
                funcionario.AtualizarTelefones(telefones);

            if (enderecos != null && enderecos.Any())
            {
                if (enderecos.Any(x => x.TipoEndereco == TipoEnderecoEnum.Comercial))
                    funcionario.AtualizarEnderecoComercial(enderecos.FirstOrDefault(x => x.TipoEndereco == TipoEnderecoEnum.Comercial));

                if (enderecos.Any(x => x.TipoEndereco == TipoEnderecoEnum.Residencial))
                    funcionario.AtualizarEnderecoResidencial(enderecos.FirstOrDefault(x => x.TipoEndereco == TipoEnderecoEnum.Residencial));
            }
            return funcionario;
        }

        public async Task<IEnumerable<Funcionario>> ObterTodos()
        {
            IEnumerable<Funcionario> data = await _repositoryRead.ObterTodos(null);
            return data;
        }

        private async Task TratarEndereco(Endereco endereco, IDbTransaction transaction)
        {
            if (endereco.Id > 0)
                await _repositoryWrite.AtualizarEndereco(endereco, transaction);
            else
                await _repositoryWrite.InserirEndereco(endereco, transaction);
        }

        private async Task TratarTelefones(IEnumerable<Telefone> telefones, IDbTransaction transaction)
        {
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
