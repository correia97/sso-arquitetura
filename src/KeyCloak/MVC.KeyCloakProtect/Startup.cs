using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.HttpsPolicy;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.IdentityModel.Protocols.OpenIdConnect;
using Microsoft.AspNetCore.Http;
using MVC.KeyCloakProtect.Interfaces;
using MVC.KeyCloakProtect.Services;
using System.IO;
using System.Diagnostics;

namespace MVC.KeyCloakProtect
{
    public class Startup
    {
        public Startup(IConfiguration configuration)
        {
            Configuration = configuration;
        }

        public IConfiguration Configuration { get; }

        // This method gets called by the runtime. Use this method to add services to the container.
        public void ConfigureServices(IServiceCollection services)
        {

            var authUrl = Configuration.GetValue<string>("authUrl");
            var redirecBaseUrl = Configuration.GetValue<string>("redirecBasetUrl");
            var clientId = Configuration.GetValue<string>("clientId");
            var clientSecret = Configuration.GetValue<string>("clientSecret");
            services.AddAuthentication(options =>
            {
                options.DefaultAuthenticateScheme = CookieAuthenticationDefaults.AuthenticationScheme;
                options.DefaultSignInScheme = CookieAuthenticationDefaults.AuthenticationScheme;
                options.DefaultChallengeScheme = CookieAuthenticationDefaults.AuthenticationScheme;

            })
            .AddCookie()
            .AddOpenIdConnect("auth", options =>
            {
                options.Authority = authUrl;
                options.ClientId = clientId;
                options.ClientSecret = clientSecret;
                options.ResponseType = OpenIdConnectResponseType.Code;
                options.Scope.Clear();
                options.Scope.Add("openid");
                options.Scope.Add("profile");
                options.Scope.Add("email");
                options.RequireHttpsMetadata = false;
                options.SaveTokens = true;
                options.CallbackPath = new PathString("/callback");
                options.ClaimsIssuer = authUrl;
                options.GetClaimsFromUserInfoEndpoint = true;
                options.Events = new Microsoft.AspNetCore.Authentication.OpenIdConnect.OpenIdConnectEvents
                {
                    OnAuthorizationCodeReceived = async context =>
                    {

                    },
                    OnTicketReceived = async context =>
                    {
                        
                        //var streamBody = new MemoryStream();
                        //context.Response.Body.Read()
                        //context.Response.Body.Seek(context.Response.Body.Length, SeekOrigin.Begin);
                        //await context.Response.Body.CopyToAsync(streamBody);
                        //using(var reader = new StreamReader(streamBody))
                        //{
                        //    var body = await reader.ReadToEndAsync();
                        //    Debugger.Log(3, "", body);
                        //}

                    },
                    OnRedirectToIdentityProvider = async context =>
                    {
                        context.ProtocolMessage.IssuerAddress = $"{redirecBaseUrl}/auth/realms/Sample/protocol/openid-connect/auth";
                    }

                };
            });



            services.AddControllersWithViews();

            services.AddScoped<IApiService, ApiService>();
        }

        // This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
        public void Configure(IApplicationBuilder app, IWebHostEnvironment env)
        {
            if (env.IsDevelopment())
            {
                app.UseDeveloperExceptionPage();
            }
            else
            {
                app.UseExceptionHandler("/Home/Error");
                // The default HSTS value is 30 days. You may want to change this for production scenarios, see https://aka.ms/aspnetcore-hsts.
                app.UseHsts();
            }
            app.UseHttpsRedirection();
            app.UseStaticFiles();

            app.UseRouting();

            app.UseCookiePolicy();
            app.UseAuthentication();
            app.UseAuthorization();

            app.UseEndpoints(endpoints =>
            {
                endpoints.MapControllerRoute(
                    name: "default",
                    pattern: "{controller=Home}/{action=Index}/{id?}");
            });
        }
    }
}
