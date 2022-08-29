using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Serilog;
using Serilog.Events;
using Serilog.Filters;
using Serilog.Formatting.Elasticsearch;
using Serilog.Sinks.RabbitMQ.Sinks.RabbitMQ;
using Serilog.Sinks.RabbitMQ;
using Microsoft.Extensions.Logging;

namespace Cadastro.Configuracoes
{
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

            Log.Logger = new LoggerConfiguration()
                      .MinimumLevel.Debug()
                      .MinimumLevel.Override("Microsoft", LogEventLevel.Information)
                      .Enrich.WithProperty("App Name", serviceName)
                      .Enrich.FromLogContext()
                      .Enrich.WithEnvironmentName()
                      .WriteTo.Logger(lc => lc.Filter.ByExcluding(Matching.WithProperty("AuditLog"))
                                      .WriteTo.Console(logFormatter)
                                      .WriteTo.RabbitMQ(rabbitCliente, rabbitConfiguration, logFormatter)
                      )
                      .CreateLogger();

            var loggerFactory = new LoggerFactory();
            loggerFactory
              .AddSerilog();

            services.AddSingleton<ILoggerFactory>(loggerFactory);

            return Log.Logger;
        }
    }
}
