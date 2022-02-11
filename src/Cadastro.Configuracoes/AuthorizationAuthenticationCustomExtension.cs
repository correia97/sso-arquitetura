using Cadastro.Domain.Entities;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.IdentityModel.Tokens;
using Newtonsoft.Json;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;

namespace Cadastro.Configuracoes
{

    public static class AuthorizationAuthenticationExtension
    {
        public static IServiceCollection AddAPICustomAuthorizationAuthenticationConfig(this IServiceCollection services, 
                                                                                            IWebHostEnvironment environment, 
                                                                                            IConfiguration configuration)
        {
            var clientId = configuration.GetValue<string>("ClientId");
            var clientSecret = configuration.GetValue<string>("ClientSecret");
            var key = new SymmetricSecurityKey(Encoding.ASCII.GetBytes(clientSecret)) { KeyId = clientId };

            var complement = configuration.GetValue<string>("UrlComplement");
            var authUrl = $"{configuration.GetValue<string>("BaseAuthUrl")}{complement}";
            var audience = configuration.GetValue<string>("Audience");


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
                                if (environment.IsDevelopment())
                                {
                                    return context.Response.WriteAsync(context.Exception.ToString());
                                }
                                return context.Response.WriteAsync("An error occured processing your authentication.");
                            }
                        };
                    });

            return services;
        }

        private static List<ClaimsIdentity> FillToken(TokenPayload payload)
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
    }
}