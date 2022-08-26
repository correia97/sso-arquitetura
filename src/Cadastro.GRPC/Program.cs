using Cadastro.Configuracoes;
using Cadastro.GRPC.Services;
using OpenTelemetry.Logs;
using OpenTelemetry.Resources;
using OpenTelemetry.Trace;

var builder = WebApplication.CreateBuilder(args);

// Additional configuration is required to successfully run gRPC on macOS.
// For instructions on how to configure Kestrel and gRPC clients on macOS, visit https://go.microsoft.com/fwlink/?linkid=2099682

// Add services to the container.
builder.Services.AddGrpc();


var serviceName = typeof(FuncionarioGrpcService).Assembly.GetName().Name;
var serviceVersion = typeof(FuncionarioGrpcService).Assembly.GetName().Version!.ToString() ?? "unknown";

builder.Services.AddCustomOpenTelemetryMetrics(serviceName, serviceVersion, builder.Configuration);
builder.Services.AddCustomOpenTelemetryTracing(serviceName, serviceVersion, builder.Configuration);
builder.Services.AddCustomOpenTelemetryLogging(serviceName, serviceVersion, builder.Logging);


var app = builder.Build();

// Configure the HTTP request pipeline.
app.MapGrpcService<GreeterService>();
app.MapGrpcService<FuncionarioGrpcService>();
app.MapGet("/", () => "Communication with gRPC endpoints must be made through a gRPC client. To learn how to create a client, visit: https://go.microsoft.com/fwlink/?linkid=2086909");

app.Run();
