using Cadastro.Configuracoes;
using Cadastro.MVC.Interfaces;
using Cadastro.MVC.Services;
using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.IdentityModel.Logging;
using MVC.Interfaces;
using MVC.Services;
using OpenTelemetry;
using OpenTelemetry.Logs;
using OpenTelemetry.Metrics;
using OpenTelemetry.Resources;
using OpenTelemetry.Trace;
using System;

System.Net.ServicePointManager.ServerCertificateValidationCallback +=
                                        (sender, certificate, chain, sslPolicyErrors) => true;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddMVCCustomAuthenticationConfig(builder.Environment, builder.Configuration);

builder.Services.AddControllersWithViews().AddRazorRuntimeCompilation();

builder.Services.AddScoped<IWeatherForecastService, WeatherForecastService>();

builder.Services.AddScoped<IFuncionarioService, FuncionarioService>();

builder.Services.AddSingleton(TracerProvider.Default.GetTracer(typeof(FuncionarioService).Name));

builder.Services.AddMVCCustomCookiePolicyOptionsConfig();

builder.Services.AddHealthChecks();

var serviceName = typeof(FuncionarioService).Assembly.GetName().Name;
var serviceVersion = typeof(FuncionarioService).Assembly.GetName().Version!.ToString() ?? "unknown";
builder.Services.AddCustomOpenTelemetryMetrics(serviceName, serviceVersion, builder.Configuration);
builder.Services.AddCustomOpenTelemetryTracing(serviceName, serviceVersion, builder.Configuration);
builder.Services.AddCustomOpenTelemetryLogging(serviceName, serviceVersion, builder.Logging);

// Add services to the container.
builder.Services.AddRazorPages();

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

//app.UseOpenTelemetryPrometheusScrapingEndpoint(); 

app.Run();