using Cadastro.Configuracoes;
using Cadastro.MVC.Interfaces;
using Cadastro.MVC.Services;
using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.IdentityModel.Logging;
using MVC.Interfaces;
using MVC.Services;
using OpenTelemetry.Metrics;
using OpenTelemetry.Resources;
using OpenTelemetry.Trace;


System.Net.ServicePointManager.ServerCertificateValidationCallback +=
                                        (sender, certificate, chain, sslPolicyErrors) => true;




var builder = WebApplication.CreateBuilder(args);

builder.Services.AddMVCCustomAuthenticationConfig(builder.Environment, builder.Configuration);

builder.Services.AddControllersWithViews().AddRazorRuntimeCompilation();

builder.Services.AddScoped<IWeatherForecastService, WeatherForecastService>();
builder.Services.AddScoped<IFuncionarioService, FuncionarioService>();

builder.Services.AddMVCCustomCookiePolicyOptionsConfig();

builder.Services.AddHealthChecks();

builder.Services.AddOpenTelemetryTracing(traceProvider =>
{
    traceProvider
        .AddSource(typeof(WeatherForecastService).Assembly.GetName().Name)
        .SetResourceBuilder(
            ResourceBuilder.CreateDefault()
                .AddService(serviceName: typeof(WeatherForecastService).Assembly.GetName().Name,
                    serviceVersion: typeof(WeatherForecastService).Assembly.GetName().Version!.ToString()))
        .AddHttpClientInstrumentation()
        .AddAspNetCoreInstrumentation()
        .AddConsoleExporter()
        .AddJaegerExporter(exporter =>
        {
            exporter.AgentHost = "jaeger";
            exporter.AgentPort = 6831;
        });
});

builder.Services.AddOpenTelemetryMetrics(config =>
{
    config
        .SetResourceBuilder(
            ResourceBuilder.CreateDefault()
                .AddService(serviceName: typeof(WeatherForecastService).Assembly.GetName().Name,
                    serviceVersion: typeof(WeatherForecastService).Assembly.GetName().Version!.ToString())
                .AddEnvironmentVariableDetector()
            .AddTelemetrySdk()
                )
    .AddPrometheusExporter(options =>
    {
        options.StartHttpListener = true;
        // Use your endpoint and port here
        options.HttpListenerPrefixes = new string[] { $"http://prometheus:{9090}/" };
        options.ScrapeResponseCacheDurationMilliseconds = 0;
    })
    .AddAspNetCoreInstrumentation()
    .AddHttpClientInstrumentation();
    // The rest of your setup code goes here too
});

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