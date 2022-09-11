using Cadastro.Configuracoes;
using Cadastro.MVC.Interfaces;
using Cadastro.MVC.Services;
using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.IdentityModel.Logging;
using MVC.Interfaces;
using MVC.Services;
using Serilog;
using System;

System.Net.ServicePointManager.ServerCertificateValidationCallback +=
                                        (sender, certificate, chain, sslPolicyErrors) => true;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddMVCCustomAuthenticationConfig(builder.Environment, builder.Configuration);

builder.Services.AddControllersWithViews().AddRazorRuntimeCompilation();

builder.Services.AddScoped<IWeatherForecastService, WeatherForecastService>();

builder.Services.AddScoped<IFuncionarioService, FuncionarioService>();

builder.Services.AddMVCCustomCookiePolicyOptionsConfig();

builder.Services.AddHealthChecks();


string serviceName = typeof(FuncionarioService).Assembly.GetName().Name;
string serviceVersion = typeof(FuncionarioService).Assembly.GetName().Version?.ToString();

// builder.Services.AddCustomOpenTelemetryTracing(serviceName, serviceVersion, builder.Configuration);
// builder.Services.AddCustomOpenTelemetryMetrics(serviceName, serviceVersion, builder.Configuration);

// Add services to the container.
builder.Services.AddRazorPages();

Log.Logger = LoggingExtension.AddCustomLogging(builder.Services, builder.Configuration, serviceName);

var app = builder.Build();

app.UseCookiePolicy();

// Configure the HTTP request pipeline.
if (app.Environment.EnvironmentName.ToUpper().Contains("PROD"))
{
    app.UseExceptionHandler("/Error");
    // The default HSTS value is 30 days. You may want to change this for production scenarios, see https://aka.ms/aspnetcore-hsts.
    //app.UseHsts();
    //app.UseHttpsRedirection();
}
else
{
    app.UseDeveloperExceptionPage();
    IdentityModelEventSource.ShowPII = true;
}

//app.UseAllElasticApm(app.Configuration);

//app.UseOpenTelemetryPrometheusScrapingEndpoint();

app.UseStaticFiles();

app.UseRouting();

app.MapRazorPages();

app.UseAuthentication();

app.UseAuthorization();

app.UseEndpoints(endpoints =>
{
    endpoints.MapControllerRoute(
        name: "default",
        pattern: "{controller=Home}/{action=Index}/{id?}");
});

app.MapHealthChecks("/health");

try
{
    Log.Information("Starting MVC Core Serilog");
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