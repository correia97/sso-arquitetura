using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using OpenTelemetry;
using OpenTelemetry.Metrics;
using OpenTelemetry.Resources;
using OpenTelemetry.Trace;
using System.Diagnostics.CodeAnalysis;

namespace Cadastro.Configuracoes
{
    [ExcludeFromCodeCoverage]
    public static class TracingMetricsExtension
    {
        public static IServiceCollection AddCustomOpenTelemetry(this IServiceCollection services, string serviceName, string serviceVersion, IConfiguration config)
        {
            services.AddOpenTelemetry()
                .WithMetrics(context =>
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
                })
                .WithTracing(traceProvider =>
                {
                    traceProvider
                     .AddSource(new string[] { serviceName })
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
                     });
                })                
                .StartWithHost();
            return services;
        }

    }
}