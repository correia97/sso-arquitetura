using Cadastro.Configuracoes;
using Cadastro.Data.Repositories;
using Cadastro.Domain.Interfaces;
using Cadastro.Domain.Services;
using Cadastro.WorkerService;
using Cadastro.WorkerServices.Migrations;
using Elastic.Apm.NetCoreAll;
using FluentMigrator.Runner;
using Microsoft.AspNetCore.Http.Json;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Npgsql;
using Serilog;
using System;
using System.Data;
using System.Diagnostics;
using System.Text.Json.Serialization;
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
        Log.Error(ex, "Erro ao rodar as migrations");
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
    catch (Exception ex)
    {
        Log.Error(ex, "Erro ao criar base de dados");
    }
}

IConfiguration configuration = default;

IHost host = Host.CreateDefaultBuilder(args)
    .ConfigureServices((context, services) =>
    {
        configuration = context.Configuration;

        string serviceName = typeof(Worker).Assembly.GetName().Name;
        string serviceVersion = typeof(Worker).Assembly.GetName().Version?.ToString();
        var activity = new ActivitySource(serviceName, serviceVersion);
        services.AddScoped<ActivitySource>(x => activity);

        // services.AddCustomOpenTelemetryTracing(serviceName, serviceVersion, configuration);
        // services.AddCustomOpenTelemetryMetrics(serviceName, serviceVersion, configuration);

        Log.Logger = LoggingExtension.AddCustomLogging(services, configuration, serviceName);

        services.AddHostedService<Worker>();

        services.AddScoped<IDbConnection>(sp =>
        {
            var connection = new NpgsqlConnection(context.Configuration.GetConnectionString("Base"));
            connection.Open();
            return connection;
        });

        services.Configure<JsonOptions>(opt =>
        {
            opt.SerializerOptions.NumberHandling = JsonNumberHandling.AllowReadingFromString;
            opt.SerializerOptions.UnknownTypeHandling = JsonUnknownTypeHandling.JsonElement;
            opt.SerializerOptions.PropertyNameCaseInsensitive = true;
            opt.SerializerOptions.Converters.Add(new ExceptionConverter());
        });

        services.AddScoped<IFuncionarioWriteRepository, FuncionarioRepository>();
        services.AddScoped<IFuncionarioReadRepository, FuncionarioRepository>();
        services.AddScoped<IFuncionarioService, FuncionarioService>();
        services.AddFluentMigratorCore();

        services.AddRabbitCustomConfiguration(configuration);
        services.ConfigureRunner(rb =>
        {
            rb.AddPostgres11_0();
            rb.WithGlobalConnectionString("Base");
            rb.ScanIn(typeof(CriarBaseMigration).Assembly).For.Migrations();
        });
    })
    //.UseAllElasticApm()
    .Build();

UpdateDatabase(host.Services, configuration);

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