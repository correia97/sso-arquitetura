using Cadastro.WorkerService;
using Cadastro.WorkerServices.Migrations;
using FluentMigrator.Runner;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Npgsql;
using RabbitMQ.Client;
using System;
using System.Threading;

void UpdateDatabase(IServiceProvider services, IConfiguration configuration)
{
    try
    {
        Thread.Sleep(TimeSpan.FromSeconds(10));

         CreateDataBase(configuration);

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

void CreateDataBase(IConfiguration configuration)
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

void SetupRabbitMQ(IConnection connection)
{
    try
    {
        IModel model = connection.CreateModel();
        QueueDeclareOk cadastrarResult = model.QueueDeclare("cadastrar", true, false, false);
        QueueDeclareOk atualizarrResult = model.QueueDeclare("atualizar", true, false, false);
        QueueDeclareOk notificarResult = model.QueueDeclare("notificar", true, false, false);
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


IConfiguration configuration = default;

IHost host = Host.CreateDefaultBuilder(args)
    .ConfigureServices((context, services) =>
    {
        configuration = context.Configuration;
        services.AddHostedService<Worker>();
        services.AddFluentMigratorCore();
        services.AddSingleton(sp =>
        {
            var factory = new ConnectionFactory();
            factory.Uri = new System.Uri(context.Configuration.GetValue<string>("rabbit"));
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
    })
    .Build();

UpdateDatabase(host.Services, configuration);


await host.RunAsync();