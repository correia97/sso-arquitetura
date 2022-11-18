using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Serilog;
using Serilog.Events;
using Serilog.Filters;
using Serilog.Formatting.Elasticsearch;
using Serilog.Sinks.RabbitMQ;
using Serilog.Sinks.RabbitMQ.Sinks.RabbitMQ;
using System.Diagnostics.CodeAnalysis;

namespace Cadastro.Configuracoes
{
    [ExcludeFromCodeCoverage]
    public static class LoggingExtension
    {
        public static Serilog.ILogger AddCustomLogging(IServiceCollection services, IConfiguration configuration, string serviceName)
        {
            var logFormatter = new ElasticsearchJsonFormatter();
            var rabbitCliente = new RabbitMQClientConfiguration();
            configuration.Bind("rabbitLog", rabbitCliente);
            rabbitCliente.SslOption = new RabbitMQ.Client.SslOption
            {
                Enabled = false,
            };

            rabbitCliente.DeliveryMode = RabbitMQDeliveryMode.Durable;

            var rabbitConfiguration = new RabbitMQSinkConfiguration
            {
                Period = TimeSpan.FromDays(1),
                TextFormatter = logFormatter,
            };

            Log.Logger = new Serilog.LoggerConfiguration()
                      .MinimumLevel.Information()
                      .MinimumLevel.Override("Microsoft", LogEventLevel.Information)
                      .Enrich.WithProperty("App Name", serviceName)
                      .Enrich.FromLogContext()
                      .Enrich.WithEnvironmentName()
                      .WriteTo.Logger(lc => lc.Filter.ByExcluding(Matching.WithProperty("AuditLog"))
                            .WriteTo.Console(logFormatter)
                            .WriteTo.RabbitMQ(rabbitCliente, rabbitConfiguration, logFormatter)
                      )
                      .CreateLogger();


            services.Configure<ILoggerFactory>(loggerFactory =>
            {
                loggerFactory.AddSerilog();
            });

            return Log.Logger;
        }


        public static void CustomLogInformation(this Microsoft.Extensions.Logging.ILogger logger, string message)
        {

            logger.LogInformation($"{message} at: {DateTime.Now:dd/MM/yyyy HH:mm:ss}");
        }

        public static void CustomLogError(this Microsoft.Extensions.Logging.ILogger logger, Exception exception, string message)
        {

            logger.LogError(exception, $"{message} at: {DateTime.Now:dd/MM/yyyy HH:mm:ss}");
        }

        public static void CustomLogError(this Microsoft.Extensions.Logging.ILogger logger, string message)
        {

            logger.LogError($"{message} at: {DateTime.Now:dd/MM/yyyy HH:mm:ss}");
        }
    }
}
