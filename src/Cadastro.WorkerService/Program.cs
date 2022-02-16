using Cadastro.Configuracoes;
using Cadastro.WorkerService;
using Cadastro.WorkerServices.Migrations;
using FluentMigrator.Runner;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Npgsql;
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

        Console.WriteLine(ex.Message);
        throw;
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



IConfiguration configuration = default;

IHost host = Host.CreateDefaultBuilder(args)
    .ConfigureServices((context, services) =>
    {
        configuration = context.Configuration;
        services.AddHostedService<Worker>();
        services.AddFluentMigratorCore();
        services.AddRabbitCustomConfiguration(context.Configuration);
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