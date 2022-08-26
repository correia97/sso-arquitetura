using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using OpenTelemetry;
using OpenTelemetry.Logs;
using OpenTelemetry.Metrics;
using OpenTelemetry.Resources;
using OpenTelemetry.Trace;
using Microsoft.Extensions.Logging;
using Microsoft.AspNetCore.Http;

namespace Cadastro.Configuracoes
{
    public static class TracingMetricsextension
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
                    .AddHttpClientInstrumentation()
                   .AddAspNetCoreInstrumentation((options) => options.Enrich
                    = (activity, eventName, rawObject) =>
                    {
                        if (eventName.Equals("OnStartActivity"))
                        {
                            if (rawObject is HttpRequest httpRequest)
                            {
                                activity.SetTag("requestProtocol", httpRequest.Protocol);
                            }
                        }
                        else if (eventName.Equals("OnStopActivity"))
                        {
                            if (rawObject is HttpResponse httpResponse)
                            {
                                activity.SetTag("responseLength", httpResponse.ContentLength);
                            }
                        }
                    })
                    .AddSqlClientInstrumentation(options =>
                    {
                        options.SetDbStatementForText = true;
                        options.RecordException = true;
                    })
                    .AddConsoleExporter()
                    .AddJaegerExporter(exporter =>
                    {
                        exporter.AgentHost = config.GetSection("jaeger:host").Value;
                        exporter.AgentPort = int.Parse(config.GetSection("jaeger:port").Value);
                        exporter.Endpoint = new Uri(config.GetSection("jaeger:url").Value);
                        exporter.Protocol = OpenTelemetry.Exporter.JaegerExportProtocol.UdpCompactThrift;
                    });
            });
            return services;
        }
        public static IServiceCollection AddCustomOpenTelemetryMetrics(this IServiceCollection services, string serviceName, string serviceVersion, IConfiguration config)
        {

            var meterProvider = Sdk.CreateMeterProviderBuilder()
                .AddPrometheusExporter(options =>
                {
                    options.StartHttpListener = true;
                    options.HttpListenerPrefixes = new string[] { $"{config.GetSection("prometheus:url").Value}:{config.GetSection("prometheus:port").Value}" };
                    options.ScrapeResponseCacheDurationMilliseconds = 0;
                })
                .Build();

            services.AddSingleton(meterProvider);

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
                .AddPrometheusExporter(options =>
                {
                    options.StartHttpListener = true;
                    // Use your endpoint and port here
                    options.HttpListenerPrefixes = new string[] { $"{config.GetSection("prometheus:url").Value}:{config.GetSection("prometheus:port").Value}" };
                    options.ScrapeResponseCacheDurationMilliseconds = 0;
                })
                .AddMeter()
                .AddAspNetCoreInstrumentation()
                .AddHttpClientInstrumentation();
                // The rest of your setup code goes here too
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
            });

            services.Configure<OpenTelemetryLoggerOptions>(opt =>
            {
                opt.IncludeScopes = true;
                opt.ParseStateValues = true;
                opt.IncludeFormattedMessage = true;
            });
            return services;
        }
    }
}
