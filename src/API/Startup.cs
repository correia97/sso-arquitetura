using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Authentication.OpenIdConnect;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.HttpsPolicy;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Microsoft.IdentityModel.Protocols.OpenIdConnect;

using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Authorization;
using API.Configuration;
using Microsoft.IdentityModel.Logging;
using Microsoft.IdentityModel.Tokens;
using System.Text;
using System.Security.Claims;
using Newtonsoft.Json;

namespace API
{
    public class Startup
    {
        public Startup(IConfiguration configuration, IWebHostEnvironment env)
        {
            Configuration = configuration;
            Environment = env;
        }
        readonly string MyAllowSpecificOrigins = "_myAllowSpecificOrigins";
        public IConfiguration Configuration { get; }
        public IWebHostEnvironment Environment { get; }

        // This method gets called by the runtime. Use this method to add services to the container.
        public void ConfigureServices(IServiceCollection services)
        {
            // if (Environment.IsDevelopment())
            // {
            IdentityModelEventSource.ShowPII = true;
            // }

            var clientId = Configuration.GetValue<string>("ClientId");
            var clientSecret = Configuration.GetValue<string>("ClientSecret");
            var key = new SymmetricSecurityKey(Encoding.ASCII.GetBytes(clientSecret)) { KeyId = clientId };

            var complement = Configuration.GetValue<string>("UrlComplement");
            var authUrl = $"{Configuration.GetValue<string>("BaseAuthUrl")}{complement}";
            var audience = Configuration.GetValue<string>("Audience");
            services.AddControllers();
            services.AddAuthorization(options =>
            {
                options.AddPolicy("email", policy => policy.Requirements.Add(new HasScopeRequirement("email", authUrl)));
            });
            services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
            .AddJwtBearer(o =>
            {
                o.Authority = authUrl;
                if (!string.IsNullOrEmpty(audience))
                    o.Audience = audience;
                o.RequireHttpsMetadata = false;
                o.SaveToken = true;
                o.ClaimsIssuer = "acme.com";
                //  o.TokenValidationParameters.IssuerSigningKey = key;
                o.TokenValidationParameters.NameClaimType = ClaimTypes.NameIdentifier;
                o.TokenValidationParameters.ValidateIssuerSigningKey = false;
                o.TokenValidationParameters.ValidateIssuer = false;
                o.TokenValidationParameters.ValidIssuer = "acme.com";
                o.Events = new JwtBearerEvents
                {
                    OnAuthenticationFailed = context =>
                    {
                        context.NoResult();
                        context.Response.StatusCode = 401;
                        context.Response.ContentType = "text/plain";
                        if (Environment.IsDevelopment())
                        {
                            return context.Response.WriteAsync(context.Exception.ToString());
                        }
                        return context.Response.WriteAsync("An error occured processing your authentication.");
                    }
                };
            });

            services.AddCors(options =>
               {
                   options.AddPolicy(name: MyAllowSpecificOrigins,
                                     builder =>
                                     {
                                         builder.AllowAnyOrigin()
                                                 .AllowAnyHeader()
                                                 .AllowAnyMethod();
                                     });
               });

            services.AddSingleton<IAuthorizationHandler, HasScopeHandler>();
        }

        // This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
        public void Configure(IApplicationBuilder app, IWebHostEnvironment env)
        {
            // if (env.IsDevelopment())
            //{
            app.UseDeveloperExceptionPage();
            // }

            app.UseAuthorization();
            app.UseAuthentication();

            app.UseHttpsRedirection();

            app.UseRouting();

            app.UseCors(MyAllowSpecificOrigins);
            app.UseAuthorization();

            app.UseEndpoints(endpoints =>
            {
                endpoints.MapControllers();
            });

        }
    }
}
