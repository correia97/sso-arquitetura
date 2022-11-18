using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using OpenTelemetry;
using OpenTelemetry.Logs;
using OpenTelemetry.Metrics;
using OpenTelemetry.Resources;
using OpenTelemetry.Trace;
using System.Diagnostics;
using System.Diagnostics.CodeAnalysis;

namespace Cadastro.Configuracoes
{
    [ExcludeFromCodeCoverage]
    public static class TracingMetricsExtension
    {
        public static IServiceCollection AddCustomOpenTelemetryTracing(this IServiceCollection services, string serviceName, string serviceVersion, IConfiguration config)
        {
            services.AddOpenTelemetryTracing(traceProvider =>
            {
                traceProvider
                 .AddSource(serviceName)
                 .SetResourceBuilder(
                     ResourceBuilder.CreateDefault()
                         .AddService(serviceName: serviceName,
                             serviceVersion: serviceVersion))
                 .AddHttpClientInstrumentation(options =>
                 {
                     options.RecordException = true;
                 })
                 .AddAspNetCoreInstrumentation()
                 .AddSqlClientInstrumentation(options =>
                 {
                     options.SetDbStatementForText = true;
                     options.RecordException = true;
                 })
                 .AddConsoleExporter()
                 .AddJaegerExporter(opt =>
                 {
                     opt.AgentHost = config.GetSection("jaeger:host").Value;
                     opt.AgentPort = int.Parse(config.GetSection("jaeger:port").Value);
                     opt.Endpoint = new Uri($"http://{opt.AgentHost}:14268/api/traces");
                     opt.ExportProcessorType = ExportProcessorType.Batch;
                     opt.BatchExportProcessorOptions = new BatchExportProcessorOptions<Activity>()
                     {
                         MaxQueueSize = 2048,
                         ScheduledDelayMilliseconds = 5000,
                         ExporterTimeoutMilliseconds = 30000,
                         MaxExportBatchSize = 512,
                     };
                 });
            });
            return services;
        }
        public static IServiceCollection AddCustomOpenTelemetryMetrics(this IServiceCollection services, string serviceName, string serviceVersion)
        {
            services.AddOpenTelemetryMetrics(context =>
            {
                context
                    .SetResourceBuilder(
                        ResourceBuilder.CreateDefault()
                            .AddService(serviceName: serviceName,
                                serviceVersion: serviceVersion)
                            .AddEnvironmentVariableDetector()
                        .AddTelemetrySdk()
                            )
                .AddMeter()
                .AddAspNetCoreInstrumentation()
                .AddHttpClientInstrumentation()
                .AddPrometheusExporter();
            });

            return services;
        }

        public static IServiceCollection AddCustomOpenTelemetryLogging(this IServiceCollection services, string serviceName, string serviceVersion, ILoggingBuilder logging)
        {

            Action<ResourceBuilder> configureResource = r => r.AddService(
                serviceName, serviceVersion: serviceVersion, serviceInstanceId: Environment.MachineName);

            logging.AddOpenTelemetry(options =>
            {
                options.ConfigureResource(configureResource);
                options.AddConsoleExporter();
                options.IncludeScopes = true;
                options.ParseStateValues = true;
                options.IncludeFormattedMessage = true;
            });

            return services;
        }
    }
}