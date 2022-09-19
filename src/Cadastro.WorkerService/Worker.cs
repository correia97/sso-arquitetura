using Cadastro.Configuracoes;
using Cadastro.Domain.Services;
using Domain.Entities;
using Microsoft.Extensions.Hosting;
using System.Threading;
using System.Threading.Tasks;

namespace Cadastro.WorkerService
{
    public class Worker : BackgroundService
    {
        //public Worker(ILogger<Worker> logger, ActivitySource activity, IModel model, IServiceProvider serviceProvider)
        //    : base(logger, activity, model, serviceProvider)
        //{
        //}
        private readonly RabbitMQConsumer mQConsumer;
        public Worker(RabbitMQConsumer consumer)
        {
            mQConsumer = consumer;
        }

        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            mQConsumer.AddConsumer<IFuncionarioService, Funcionario>((svc, msg) => svc.Cadastrar(msg), "cadastrar");

            mQConsumer.AddConsumer<IFuncionarioService, Funcionario>((svc, msg) => svc.Atualizar(msg), "atualizar");

            await Task.Delay(1000, stoppingToken);
        }
    }
}
