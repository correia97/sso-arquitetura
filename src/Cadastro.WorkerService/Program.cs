using Cadastro.Configuracoes;
using Cadastro.Data.Repositories;
using Cadastro.Domain.Interfaces;
using Cadastro.Domain.Services;
using Cadastro.WorkerService;
using Cadastro.WorkerServices.Migrations;
using Elastic.Apm.NetCoreAll;
using FluentMigrator.Runner;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Npgsql;
using Serilog;
using System;
using System.Data;
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
        using var m_conn = new NpgsqlConnection(connStr);
        using var m_createdb_cmd = new NpgsqlCommand(@"CREATE DATABASE funcionarios", m_conn);
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

        Log.Logger = LoggingExtension.AddCustomLogging(services, configuration, typeof(Worker).Assembly.FullName);

        services.AddHostedService<Worker>();

        services.AddScoped<IDbConnection>(sp =>
        {
            var connection = new NpgsqlConnection(context.Configuration.GetConnectionString("Base"));
            connection.Open();
            return connection;
        });
        services.AddScoped<IFuncionarioWriteRepository, FuncionarioRepository>();
        services.AddScoped<IFuncionarioReadRepository, FuncionarioRepository>();
        services.AddScoped<IFuncionarioService, FuncionarioService>();
        services.AddFluentMigratorCore();
        services.AddRabbitCustomConfiguration(context.Configuration);
        services.ConfigureRunner(rb =>
        {
            rb.AddPostgres11_0();
            rb.WithGlobalConnectionString("Base");
            rb.ScanIn(typeof(CriarBaseMigration).Assembly).For.Migrations();
        });
    })
    .UseAllElasticApm()
    .Build();

UpdateDatabase(host.Services, configuration);

//AppContext.SetSwitch("Npgsql.EnableLegacyTimestampBehavior", true);

try
{
    Log.Information("Starting WorkerService Core Serilog");
    await host.RunAsync();
}
catch (Exception ex)
{
    Log.Fatal(ex, "Host terminated unexpectedly");
    return;
}
finally
{
    Log.CloseAndFlush();
}