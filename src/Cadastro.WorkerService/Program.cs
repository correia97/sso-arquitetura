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
using OpenTelemetry.Resources;
using OpenTelemetry.Trace;
using Cadastro.Domain.Interfaces;
using Cadastro.Data.Repositories;

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
        services.AddTransient<IFuncionarioWriteRepository, FuncionarioRepository>();
        services.AddFluentMigratorCore();
        services.AddRabbitCustomConfiguration(context.Configuration);
        services.ConfigureRunner(rb =>
        {
            rb.AddPostgres11_0();
            rb.WithGlobalConnectionString("Base");
            rb.ScanIn(typeof(CriarBaseMigration).Assembly).For.Migrations();
        });
        services.AddOpenTelemetryTracing(traceProvider =>
        {
            traceProvider
                .AddSource(typeof(Worker).Assembly.GetName().Name)
                .SetResourceBuilder(
                    ResourceBuilder.CreateDefault()
                        .AddService(serviceName: typeof(Worker).Assembly.GetName().Name,
                            serviceVersion: typeof(Worker).Assembly.GetName().Version!.ToString()))
                .AddHttpClientInstrumentation()
                .AddAspNetCoreInstrumentation()
                .AddSqlClientInstrumentation()
                .AddConsoleExporter();
            //.AddJaegerExporter(exporter =>
            //{
            //    exporter.AgentHost = builder.Configuration["Jaeger:AgentHost"];
            //    exporter.AgentPort = Convert.ToInt32(builder.Configuration["Jaeger:AgentPort"]);
            //});
        });
    })
    .Build();

UpdateDatabase(host.Services, configuration);

AppContext.SetSwitch("Npgsql.EnableLegacyTimestampBehavior", true);
await host.RunAsync();