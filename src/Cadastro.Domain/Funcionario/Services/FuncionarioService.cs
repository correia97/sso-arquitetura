using Cadastro.Domain.Entities;
using Cadastro.Domain.Enums;
using Cadastro.Domain.Interfaces;
using Domain.Entities;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Threading.Tasks;

namespace Cadastro.Domain.Services
{
    public class FuncionarioService : IFuncionarioService
    {
        private readonly IFuncionarioReadRepository _repositoryRead;
        private readonly IFuncionarioWriteRepository _repositoryWrite;
        private readonly INotificationService _notificationService;
        private readonly ILogger<FuncionarioService> _logger;
        private readonly ActivitySource _activitySource;
        public FuncionarioService(IFuncionarioReadRepository repositoryRead, IFuncionarioWriteRepository repositoryWrite,
                                 INotificationService notificationService, ILogger<FuncionarioService> logger, ActivitySource activitySource)
        {
            _repositoryRead = repositoryRead;
            _repositoryWrite = repositoryWrite;
            _notificationService = notificationService;
            _logger = logger;
            _activitySource = activitySource;
        }
        public async Task Atualizar(Funcionario funcionario)
        {
            using var activity = _activitySource.StartActivity("Atualizar Funcionario", ActivityKind.Internal);

            Funcionario baseFuncionario = await _repositoryRead.ObterPorId(funcionario.Id);
            baseFuncionario.Atualizar(funcionario.Nome, funcionario.DataNascimento, funcionario.Email, funcionario.Matricula, funcionario.Cargo);
            baseFuncionario.AtualizarTelefones(funcionario.Telefones);
            baseFuncionario.AtualizarEnderecoComercial(funcionario.EnderecoComercial);
            baseFuncionario.AtualizarEnderecoResidencial(funcionario.EnderecoResidencial);

            _repositoryWrite.IniciarTransacao();
            var result = await _repositoryWrite.Atualizar(baseFuncionario);

            if (!result)
            {
                _repositoryWrite.CancelarTransacao();
                _notificationService.SendEvent(new NotificationMessage(funcionario.Id, funcionario.Id, "Atualizar", false));
                throw new InvalidOperationException("Deu muito ruim!");
            }

            if (funcionario.Telefones != null && funcionario.Telefones.Any())
                await TratarTelefones(funcionario.Telefones);

            if (funcionario.EnderecoResidencial != null && !string.IsNullOrEmpty(funcionario.EnderecoResidencial.Rua))
                await TratarEndereco(funcionario.EnderecoResidencial);

            if (funcionario.EnderecoComercial != null && !string.IsNullOrEmpty(funcionario.EnderecoComercial.Rua))
                await TratarEndereco(funcionario.EnderecoComercial);

             _notificationService.SendEvent(new NotificationMessage(funcionario.Id, funcionario.Id, "Atualizar", true));
            _repositoryWrite.CompletarTransacao();
        }

        public async Task Cadastrar(Funcionario funcionario)
        {
            using var activity = _activitySource.StartActivity("Cadastrar Funcionario", ActivityKind.Internal);

            Funcionario data = await _repositoryRead.ObterPorEmail(funcionario.Email.EnderecoEmail);

            if (data != null)
                throw new InvalidOperationException("Funcionario já existe");

            _repositoryWrite.IniciarTransacao();
            var result = await _repositoryWrite.Inserir(funcionario);

            if (!result)
            {
                _repositoryWrite.CancelarTransacao();
                _notificationService.SendEvent(new NotificationMessage(funcionario.Id, funcionario.Id, "Cadastrar", false));
                throw new InvalidOperationException("Deu muito ruim!");
            }

            if (funcionario.Telefones != null && funcionario.Telefones.Any())
            {
                foreach (var item in funcionario.Telefones)
                    await _repositoryWrite.InserirTelefone(item);
            }

            if (funcionario.EnderecoResidencial != null && !string.IsNullOrEmpty(funcionario.EnderecoResidencial.Rua))
                await _repositoryWrite.InserirEndereco(funcionario.EnderecoResidencial);


            if (funcionario.EnderecoComercial != null && !string.IsNullOrEmpty(funcionario.EnderecoComercial.Rua))
                await _repositoryWrite.InserirEndereco(funcionario.EnderecoComercial);

             _notificationService.SendEvent(new NotificationMessage(funcionario.Id, funcionario.Id, "Cadastrar", true));
            _repositoryWrite.CompletarTransacao();
        }

        public async Task<Funcionario> ObterPorId(Guid id)
        {
            using var activity = _activitySource.StartActivity("Obter Funcionario por Id", ActivityKind.Internal);
            Funcionario funcionario = await _repositoryRead.ObterPorId(id);

            var enderecos = await _repositoryRead.ObterEnderecosPorFuncionarioId(id);
            var telefones = await _repositoryRead.ObterTelefonesPorFuncionarioId(id);

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
            using var activity = _activitySource.StartActivity("Obter Funcionarios", ActivityKind.Internal);
            IEnumerable<Funcionario> data = await _repositoryRead.ObterTodos();
            return data;
        }

        private async Task TratarEndereco(Endereco endereco)
        {
            using var activity = _activitySource.StartActivity("Atualizar/Inserir Endereço", ActivityKind.Internal);
            if (endereco.Id > 0)
                await _repositoryWrite.AtualizarEndereco(endereco);
            else
                await _repositoryWrite.InserirEndereco(endereco);
        }

        private async Task TratarTelefones(IEnumerable<Telefone> telefones)
        {
            using var activity = _activitySource.StartActivity("Atualizar/Inserir Telefones", ActivityKind.Internal);
            foreach (var item in telefones)
            {
                if (item.Id > 0)
                    await _repositoryWrite.AtualizarTelefone(item);
                else
                    await _repositoryWrite.InserirTelefone(item);
            }
        }
    }
}
