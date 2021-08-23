using Cadastro.WorkerServices.Migrations;
using FluentMigrator.Runner;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Npgsql;
using RabbitMQ.Client;
using System;
using System.Threading;

namespace Cadastro.WorkerService
{
    public class Program
    {
        private static IConfiguration configuration;
        public static void Main(string[] args)
        {
            var host = CreateHostBuilder(args).Build();

            UpdateDatabase(host.Services);

            host.Run();

        }

        public static IHostBuilder CreateHostBuilder(string[] args) =>
            Host.CreateDefaultBuilder(args)
                .ConfigureServices((hostContext, services) =>
                {
                    configuration = hostContext.Configuration;
                    services.AddHostedService<Worker>();
                    services.AddFluentMigratorCore();
                    services.AddSingleton(sp =>
                    {
                        ConnectionFactory factory = new ConnectionFactory();
                        factory.Uri = new System.Uri(configuration.GetValue<string>("rabbit"));
                        IConnection connection = factory.CreateConnection();
                        SetupRabbitMQ(connection);
                        return connection;
                    });
                    services.ConfigureRunner(rb =>
                    {
                        rb.AddPostgres11_0();
                        rb.WithGlobalConnectionString("Base");
                        rb.ScanIn(typeof(CriarBaseMigration).Assembly).For.Migrations();
                    });
                });

        private static void UpdateDatabase(IServiceProvider services)
        {
            try
            {
                Thread.Sleep(TimeSpan.FromSeconds(10));

                CreateDataBase();

                // Instantiate the runner
                var runner = services.GetRequiredService<IMigrationRunner>();

                // Execute the migrations
                runner.MigrateUp();
            }
            catch (Exception ex)
            {

                throw ex;
            }
        }

        private static void CreateDataBase()
        {
            try
            {

                string connStr = configuration.GetConnectionString("Base");
                connStr = connStr.Replace("Database=funcionarios;", "");
                var m_conn = new NpgsqlConnection(connStr);
                var m_createdb_cmd = new NpgsqlCommand(@"CREATE DATABASE funcionarios", m_conn);
                m_conn.Open();
                m_createdb_cmd.ExecuteNonQuery();
                m_conn.Close();
            }
            catch (PostgresException ex)
            {
                Console.WriteLine(ex.Message);
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message);
            }
        }


        public static void SetupRabbitMQ(IConnection connection)
        {
            try
            {
                IModel model = connection.CreateModel();
                var cadastrarResult = model.QueueDeclare("cadastrar", true);
                var atualizarrResult = model.QueueDeclare("atualizar", true);
                var notificarResult = model.QueueDeclare("notificar", true);
                model.ExchangeDeclare("cadastro", ExchangeType.Topic, true);

                model.ExchangeDeclare("evento", ExchangeType.Fanout, true);
                model.QueueBind("cadastrar", "cadastro", "cadastrar");
                model.QueueBind("atualizar", "cadastro", "atualizar");
                model.QueueBind("notificar", "evento", "");
            }
            catch (System.Exception ex)
            {

                throw;
            }


        }
    }
}
