using Cadastro.API.Interfaces;
using Cadastro.API.Services;
using Cadastro.Configuracoes;
using Cadastro.Data.Repositories;
using Cadastro.Data.Services;
using Cadastro.Domain.Interfaces;
using Cadastro.Domain.Services;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Mvc.ApiExplorer;
using Microsoft.AspNetCore.Mvc.Versioning;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.Logging;
using Microsoft.OpenApi.Models;
using Npgsql;
using Serilog;
using Swashbuckle.AspNetCore.SwaggerUI;
using System;
using System.Data;
using System.Diagnostics;
using System.Text.Json.Serialization;

string MyAllowSpecificOrigins = "_myAllowSpecificOrigins";

var builder = WebApplication.CreateBuilder(args);


// Add services to the container.
builder.Services.AddControllers()
                .AddJsonOptions(opt =>
                                {
                                    opt.JsonSerializerOptions.NumberHandling = JsonNumberHandling.AllowReadingFromString;
                                    opt.JsonSerializerOptions.UnknownTypeHandling = JsonUnknownTypeHandling.JsonElement;
                                    opt.JsonSerializerOptions.PropertyNameCaseInsensitive = true;
                                    opt.JsonSerializerOptions.Converters.Add(new ExceptionConverter());
                                    opt.JsonSerializerOptions.Converters.Add(new ByteArrayConverter());

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
            Array.Empty<string>()
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

builder.Services.AddScoped<IDbConnection>(sp =>
{
    var connection = new NpgsqlConnection(builder.Configuration.GetConnectionString("Base"));
    return connection;
});

builder.Services.AddSingleton<IAuthorizationHandler, HasScopeHandler>();

builder.Services.AddScoped<IFuncionarioReadRepository, FuncionarioRepository>();

builder.Services.AddScoped<IFuncionarioWriteRepository, FuncionarioRepository>();

builder.Services.AddScoped<IFuncionarioAppService, FuncionarioAppService>();

builder.Services.AddScoped<IFuncionarioService, FuncionarioService>();

builder.Services.AddScoped<INotificationService, NotificationService>();

builder.Services.AddScoped<RabbitMQConsumer>();

builder.Services.AddRabbitCustomConfiguration(builder.Configuration);

string serviceName = typeof(FuncionarioAppService).Assembly.GetName().Name;
string serviceVersion = typeof(FuncionarioAppService).Assembly.GetName().Version?.ToString();

builder.Services.AddCustomOpenTelemetry(serviceName, serviceVersion, builder.Configuration);

var activity = new ActivitySource(serviceName, serviceVersion);
builder.Services.AddScoped<ActivitySource>(x => activity);

builder.Services.AddHttpClient(Options.DefaultName);

Log.Logger = LoggingExtension.AddCustomLogging(builder.Services, builder.Configuration, serviceName);

var app = builder.Build();

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
        c.SwaggerEndpoint($"/swagger/{description.GroupName}/swagger.yaml",
                          $"Cadastro API - {app.Environment.EnvironmentName} {description.GroupName.ToUpperInvariant()}");
    }
    c.DocExpansion(DocExpansion.List);
});

app.UseHealthChecks("/health");

app.UseStaticFiles();

app.UseRouting();

app.UseOpenTelemetryPrometheusScrapingEndpoint();

app.UseCors(MyAllowSpecificOrigins);

app.UseAuthentication();

app.UseAuthorization();

app.MapControllers();

try
{
    Log.Information("Starting API Core Serilog");
    app.Run();

}
catch (Exception ex)
{
    Log.Fatal(ex, "Host terminated unexpectedly");
    return;
}
finally
{
    Log.CloseAndFlush();
}