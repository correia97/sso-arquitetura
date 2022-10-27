﻿using Microsoft.AspNetCore.Http;
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
                // .AddGrpcCoreInstrumentation()
                .AddHttpClientInstrumentation()
                .AddAspNetCoreInstrumentation()
                .AddSqlClientInstrumentation()
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

            services.Configure<AspNetCoreInstrumentationOptions>((options)
                    => options.Enrich
                    = (activity, eventName, rawObject) =>
                    {
                        switch (eventName)
                        {
                            case "OnStartActivity":
                                if (rawObject is HttpRequest httpRequest)
                                    activity.SetTag("requestProtocol", httpRequest.Protocol);

                                break;
                            case "OnStopActivity":
                                if (rawObject is HttpResponse httpResponse)
                                    activity.SetTag("responseLength", httpResponse.ContentLength);

                                break;
                        }
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