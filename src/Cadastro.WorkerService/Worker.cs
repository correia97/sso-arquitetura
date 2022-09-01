using Cadastro.Domain.Services;
using Domain.Entities;
using Microsoft.Extensions.Logging;
using RabbitMQ.Client;
using System;
using System.Threading;
using System.Threading.Tasks;

namespace Cadastro.WorkerService
{
    public class Worker : RabbitMQWorkerService
    {
        public Worker(ILogger<Worker> logger, IModel model, IServiceProvider serviceProvider)
            : base(logger, model, serviceProvider)
        {
        }

        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            this.AddConsumer<IFuncionarioService, Funcionario>((svc, msg) => svc.Cadastrar(msg), "cadastrar");

            this.AddConsumer<IFuncionarioService, Funcionario>((svc, msg) => svc.Atualizar(msg), "atualizar");

            await Task.Delay(1000, stoppingToken);
        }
    }
}
