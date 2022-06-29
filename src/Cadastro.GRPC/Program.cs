using Cadastro.GRPC.Services;
using OpenTelemetry.Resources;
using OpenTelemetry.Trace;

var builder = WebApplication.CreateBuilder(args);

// Additional configuration is required to successfully run gRPC on macOS.
// For instructions on how to configure Kestrel and gRPC clients on macOS, visit https://go.microsoft.com/fwlink/?linkid=2099682

// Add services to the container.
builder.Services.AddGrpc();



builder.Services.AddOpenTelemetryTracing(traceProvider =>
{
    traceProvider
        .AddSource(typeof(FuncionarioGrpcService).Assembly.GetName().Name)
        .SetResourceBuilder(
            ResourceBuilder.CreateDefault()
                .AddService(serviceName: typeof(FuncionarioGrpcService).Assembly.GetName().Name,
                    serviceVersion: typeof(FuncionarioGrpcService).Assembly.GetName().Version!.ToString()))
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

var app = builder.Build();

// Configure the HTTP request pipeline.
app.MapGrpcService<GreeterService>();
app.MapGrpcService<FuncionarioGrpcService>();
app.MapGet("/", () => "Communication with gRPC endpoints must be made through a gRPC client. To learn how to create a client, visit: https://go.microsoft.com/fwlink/?linkid=2086909");

app.Run();
