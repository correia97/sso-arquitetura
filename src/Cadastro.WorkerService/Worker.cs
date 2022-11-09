using Cadastro.Configuracoes;
using Cadastro.Domain.Services;
using Domain.Entities;
using Microsoft.Extensions.Hosting;
using System;
using System.Threading;
using System.Threading.Tasks;

namespace Cadastro.WorkerService
{
    public class Worker : BackgroundService
    {
        private readonly RabbitMQConsumer mQConsumer;
        public Worker(RabbitMQConsumer consumer)
        {
            mQConsumer = consumer;
        }

        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            mQConsumer.AddConsumer<IFuncionarioService, Funcionario>((svc, msg) => svc.Cadastrar(msg), "cadastrar");

            mQConsumer.AddConsumer<IFuncionarioService, Funcionario>((svc, msg) => svc.Atualizar(msg), "atualizar");

            mQConsumer.AddConsumer<IFuncionarioService, Guid>((svc, msg) => svc.Desativar(msg), "desativar");

            mQConsumer.AddConsumer<IFuncionarioService, Guid>((svc, msg) => svc.Remover(msg), "remover");

            await Task.Delay(1000, stoppingToken);
        }
    }
}
