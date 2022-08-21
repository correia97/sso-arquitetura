using Cadastro.API.Interfaces;
using Cadastro.API.Services;
using Cadastro.Configuracoes;
using Cadastro.Data.Repositories;
using Cadastro.Domain.Interfaces;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Mvc.ApiExplorer;
using Microsoft.AspNetCore.Mvc.Versioning;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.IdentityModel.Logging;
using Microsoft.OpenApi.Models;
using OpenTelemetry.Metrics;
using OpenTelemetry.Resources;
using OpenTelemetry.Trace;
using Swashbuckle.AspNetCore.SwaggerUI;
using System;
using System.Text.Json.Serialization;

string MyAllowSpecificOrigins = "_myAllowSpecificOrigins";

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.


builder.Services.AddControllers()
                .AddJsonOptions(opt =>
                                {
                                    opt.JsonSerializerOptions.NumberHandling = JsonNumberHandling.AllowReadingFromString;
                                    opt.JsonSerializerOptions.UnknownTypeHandling = JsonUnknownTypeHandling.JsonElement;
                                });



builder.Services.AddApiVersioning(x =>
{
    x.DefaultApiVersion = new Microsoft.AspNetCore.Mvc.ApiVersion(1, 0);
    x.AssumeDefaultVersionWhenUnspecified = true;
    x.ReportApiVersions = true;
    x.ApiVersionReader = new UrlSegmentApiVersionReader();
});
builder.Services.AddVersionedApiExplorer(p =>
{
    p.GroupNameFormat = "'v'VVV";
    p.SubstituteApiVersionInUrl = true;
    p.AddApiVersionParametersWhenVersionNeutral = true;
    p.AssumeDefaultVersionWhenUnspecified = true;
});
// Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
builder.Services.AddEndpointsApiExplorer();

System.Net.ServicePointManager.ServerCertificateValidationCallback +=
                        (sender, certificate, chain, sslPolicyErrors) => true;

builder.Services.AddAPICustomAuthorizationConfig(builder.Configuration);

builder.Services.AddAPICustomAuthenticationConfig(builder.Environment, builder.Configuration);

builder.Services.AddSwaggerGen(c =>
{
    c.SwaggerDoc("v1", new OpenApiInfo { Title = $"Cadastro API - {builder.Environment.EnvironmentName}", Version = "v1" });
    c.AddSecurityDefinition("Bearer", new OpenApiSecurityScheme()
    {
        Name = "Authorization",
        Type = SecuritySchemeType.ApiKey,
        Scheme = JwtBearerDefaults.AuthenticationScheme,
        BearerFormat = "JWT",
        In = ParameterLocation.Header,
        Description = @"JWT Authorization header Ex.: 'Bearer 12345abcdef'",
    });
    c.AddSecurityRequirement(new OpenApiSecurityRequirement
    {
        {
            new OpenApiSecurityScheme
            {
                Reference = new OpenApiReference
                {
                    Type = ReferenceType.SecurityScheme,
                    Id = JwtBearerDefaults.AuthenticationScheme
                }
            },
            new string[] {}
        }
    });
});

builder.Services.AddCors(options =>
{
    options.AddPolicy(name: MyAllowSpecificOrigins,
                      builder =>
                      {
                          builder.AllowAnyOrigin()
                                  .AllowAnyHeader()
                                  .AllowAnyMethod();
                      });
    options.DefaultPolicyName = MyAllowSpecificOrigins;
});


builder.Services.AddHealthChecks();
var jaeger = builder.Configuration.GetSection("jaeger");

builder.Services.AddOpenTelemetryTracing(traceProvider =>
{
    traceProvider
        .AddSource(typeof(FuncionarioAppService).Assembly.GetName().Name)
        .SetResourceBuilder(
                ResourceBuilder.CreateDefault()
                .AddService(serviceName: typeof(FuncionarioAppService).Assembly.GetName().Name,
                            serviceVersion: typeof(FuncionarioAppService).Assembly.GetName().Version!.ToString()))
        .AddHttpClientInstrumentation()
        .AddAspNetCoreInstrumentation()
        .AddSqlClientInstrumentation(
            options =>
            {
                options.SetDbStatementForText = true;
                options.RecordException = true;
            })
        .AddConsoleExporter()
        .AddJaegerExporter(exporter =>
        {
            exporter.AgentHost = builder.Configuration.GetSection("jaeger:host").Value;
            exporter.AgentPort = int.Parse(builder.Configuration.GetSection("jaeger:port").Value); 
            exporter.Endpoint = new Uri(builder.Configuration.GetSection("jaeger:url").Value);
            exporter.Protocol = OpenTelemetry.Exporter.JaegerExportProtocol.HttpBinaryThrift;
        });
});

builder.Services.AddOpenTelemetryMetrics(config =>
{
    config
        .SetResourceBuilder(
            ResourceBuilder.CreateDefault()
                .AddService(serviceName: typeof(FuncionarioAppService).Assembly.GetName().Name,
                            serviceVersion: typeof(FuncionarioAppService).Assembly.GetName().Version!.ToString())
                .AddEnvironmentVariableDetector()
                .AddTelemetrySdk()
                )
        .AddPrometheusExporter(options =>
        {
            options.StartHttpListener = true;
            options.HttpListenerPrefixes = new string[] {$"{builder.Configuration.GetSection("prometheus:url").Value}:{builder.Configuration.GetSection("prometheus:port").Value}" };
            options.ScrapeResponseCacheDurationMilliseconds = 0;
        })
        .AddAspNetCoreInstrumentation()
        .AddHttpClientInstrumentation();
});

builder.Services.AddSingleton<IAuthorizationHandler, HasScopeHandler>();

builder.Services.AddScoped<IFuncionarioReadRepository, FuncionarioRepository>();

builder.Services.AddScoped<IFuncionarioAppService, FuncionarioAppService>();

builder.Services.AddRabbitCustomConfiguration(builder.Configuration);

builder.Services.AddLogging();

var app = builder.Build();

// Configure the HTTP request pipeline.
if (!app.Environment.EnvironmentName.ToUpper().Contains("PROD"))
{
    app.UseHttpLogging();
    app.UseDeveloperExceptionPage();
    IdentityModelEventSource.ShowPII = true;
}

app.UseSwagger();

var provider = app.Services.GetRequiredService<IApiVersionDescriptionProvider>();

app.UseSwaggerUI(c =>
{
    foreach (var description in provider.ApiVersionDescriptions)
    {
        c.SwaggerEndpoint($"/swagger/{description.GroupName}/swagger.json",
                          $"Cadastro API - {app.Environment.EnvironmentName} {description.GroupName.ToUpperInvariant()}");
    }
    c.DocExpansion(DocExpansion.List);
});

//app.UseHttpsRedirection();

app.UseStaticFiles();

app.UseRouting();

app.UseCors(MyAllowSpecificOrigins);

app.UseAuthentication();

app.UseAuthorization();

app.MapHealthChecks("/health");

app.MapControllers();

app.Run();
