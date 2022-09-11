using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using OpenTelemetry;
using OpenTelemetry.Exporter;
using OpenTelemetry.Instrumentation.AspNetCore;
using OpenTelemetry.Instrumentation.SqlClient;
using OpenTelemetry.Logs;
using OpenTelemetry.Metrics;
using OpenTelemetry.Resources;
using OpenTelemetry.Trace;

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
                    .AddGrpcCoreInstrumentation()
                    .AddHttpClientInstrumentation()
                    .AddAspNetCoreInstrumentation()
                    .AddSqlClientInstrumentation()
                    .AddConsoleExporter()
                    .AddJaegerExporter();
            });

            services.Configure<AspNetCoreInstrumentationOptions>((options)
                    => options.Enrich
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
                    });

            services.Configure<JaegerExporterOptions>(exporter =>
            {

                exporter.AgentHost = config.GetSection("jaeger:host").Value;
                exporter.AgentPort = int.Parse(config.GetSection("jaeger:port").Value);
                // exporter.Endpoint = new Uri(config.GetSection("jaeger:url").Value);
                //exporter.Protocol = OpenTelemetry.Exporter.JaegerExportProtocol.UdpCompactThrift;
            });

            services.Configure<SqlClientInstrumentationOptions>(options =>
            {
                options.SetDbStatementForText = true;
                options.RecordException = true;

            });

            return services;
        }
        public static IServiceCollection AddCustomOpenTelemetryMetrics(this IServiceCollection services, string serviceName, string serviceVersion, IConfiguration config)
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
                // The rest of your setup code goes here too
            });

            var meterProvider = Sdk.CreateMeterProviderBuilder()
                .AddPrometheusExporter()
                .Build();

            services.Configure<PrometheusExporterOptions>(conf =>
            {
                conf.StartHttpListener = true;
                conf.HttpListenerPrefixes = new string[] { $"{config.GetSection("prometheus:url").Value}:{config.GetSection("prometheus:port").Value}" };
                conf.ScrapeResponseCacheDurationMilliseconds = 0;
                conf.ScrapeEndpointPath = "/metrics";
            });

            services.AddSingleton(meterProvider);

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