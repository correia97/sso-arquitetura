using Cadastro.Configuracoes;
using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.IdentityModel.Logging;
using MVC.Interfaces;
using MVC.Services;

System.Net.ServicePointManager.ServerCertificateValidationCallback +=
                                        (sender, certificate, chain, sslPolicyErrors) => true;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddMVCCustomAuthenticationConfig(builder.Environment, builder.Configuration);

builder.Services.AddControllersWithViews();

builder.Services.AddScoped<IApiService, ApiService>();

builder.Services.AddMVCCustomCookiePolicyOptionsConfig();

// Add services to the container.
builder.Services.AddRazorPages();

var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.EnvironmentName.ToUpper().Contains("PROD"))
{
    app.UseExceptionHandler("/Error");
    // The default HSTS value is 30 days. You may want to change this for production scenarios, see https://aka.ms/aspnetcore-hsts.
    app.UseHsts();
}
else
{
    app.UseDeveloperExceptionPage();
    IdentityModelEventSource.ShowPII = true;
}

app.UseHttpsRedirection();

app.UseStaticFiles();

app.UseRouting();

app.UseAuthorization();

app.MapRazorPages();

app.UseCookiePolicy();

app.UseAuthentication();

app.UseAuthorization();

app.UseEndpoints(endpoints =>
{
    endpoints.MapControllerRoute(
        name: "default",
        pattern: "{controller=Home}/{action=Index}/{id?}");
});

app.Run();