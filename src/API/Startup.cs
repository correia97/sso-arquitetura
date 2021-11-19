using API.Configuration;
using Cadastro.API.Interfaces;
using Cadastro.API.Services;
using Cadastro.Data.Repositories;
using Cadastro.Domain.Entities;
using Cadastro.Domain.Interfaces;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.IdentityModel.Logging;
using Microsoft.IdentityModel.Tokens;
using Microsoft.OpenApi.Models;
using Newtonsoft.Json;
using RabbitMQ.Client;
using System.Collections.Generic;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using System.Threading.Tasks;

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
            System.Net.ServicePointManager.ServerCertificateValidationCallback +=
                                    (sender, certificate, chain, sslPolicyErrors) => true;

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
                    OnTokenValidated = context =>
                    {
                        var token = (JwtSecurityToken)context.SecurityToken;
                        var payload = JsonConvert.DeserializeObject<TokenPayload>(token.Payload.SerializeToJson());
                        context.Principal.AddIdentities(FillToken(payload));
                        return Task.CompletedTask;
                    },
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

            services.AddSwaggerGen(c =>
            {
                c.SwaggerDoc("v1", new OpenApiInfo { Title = $"Cadastro API - {Environment.EnvironmentName}", Version = "v1" });
                c.AddSecurityDefinition("Bearer", new OpenApiSecurityScheme()
                {
                    Name = "Authorization",
                    Type = SecuritySchemeType.ApiKey,
                    Scheme = "Bearer",
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
                                  Id = "Bearer"
                              }
                          },
                         new string[] {}
                    }
                });
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
                   options.DefaultPolicyName = MyAllowSpecificOrigins;
               });

            services.AddSingleton<IAuthorizationHandler, HasScopeHandler>();

            services.AddScoped<IFuncionarioReadRepository, FuncionarioRepository>();

            services.AddScoped<IFuncionarioAppService, FuncionarioAppService>();

            services.AddSingleton(sp =>
            {
                ConnectionFactory factory = new ConnectionFactory();
                factory.Uri = new System.Uri(this.Configuration.GetValue<string>("rabbit"));
                IConnection connection = factory.CreateConnection();
                SetupRabbitMQ(connection);
                return connection;
            });

            services.AddLogging();
        }

        private List<ClaimsIdentity> FillToken(TokenPayload payload)
        {

            var claims = new List<Claim>();
            claims.Add(new Claim(ClaimTypes.GivenName, payload.given_name));
            claims.Add(new Claim(ClaimTypes.Name, payload.name));
            claims.Add(new Claim(ClaimTypes.Email, payload.email));
            claims.Add(new Claim(ClaimTypes.Surname, payload.family_name));

            foreach (var item in payload.group)
            {
                claims.Add(new Claim(ClaimTypes.Role, item));
            }
            foreach (var item in payload.realm_access.roles)
            {
                claims.Add(new Claim(ClaimTypes.Role, item));
            }
            foreach (var item in payload.resource_access.account.roles)
            {
                claims.Add(new Claim(ClaimTypes.Role, item));
            }

            var identity = new ClaimsIdentity(claims);
            return new List<ClaimsIdentity> { identity };
        }

        // This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
        public void Configure(IApplicationBuilder app, IWebHostEnvironment env)
        {
            // if (env.IsDevelopment())
            //{
            app.UseDeveloperExceptionPage();
            // }

            app.UseSwagger();
            app.UseSwaggerUI(c =>
            {
                c.SwaggerEndpoint("/swagger/v1/swagger.json", $"Cadastro API - {Environment.EnvironmentName} V1");
            });

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

        public void SetupRabbitMQ(IConnection connection)
        {
            try
            {

                IModel model = connection.CreateModel();
                var cadastrarResult = model.QueueDeclare("cadastrar", true, false, false);
                var atualizarrResult = model.QueueDeclare("atualizar", true, false, false);
                var notificarResult = model.QueueDeclare("notificar", true, false, false);
                model.ExchangeDeclare("cadastro", ExchangeType.Topic, true);

                model.ExchangeDeclare("evento", ExchangeType.Fanout, true);
                model.QueueBind("cadastrar", "cadastro", "cadastrar");
                model.QueueBind("atualizar", "cadastro", "atualizar");
                model.QueueBind("notificar", "evento", "");
            }
            catch (System.Exception ex)
            {
                throw ex;
            }


        }

    }
}
