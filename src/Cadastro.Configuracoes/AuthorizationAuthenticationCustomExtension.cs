using Cadastro.Domain.Entities;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Authentication.OpenIdConnect;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.IdentityModel.Protocols.OpenIdConnect;
using Microsoft.IdentityModel.Tokens;
using Newtonsoft.Json;
using System.Diagnostics;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;

namespace Cadastro.Configuracoes
{

    public static class AuthorizationAuthenticationExtension
    {
        public static IServiceCollection AddAPICustomAuthorizationConfig(this IServiceCollection services, IConfiguration configuration)
        {
            var complement = configuration.GetValue<string>("UrlComplement");
            var authUrl = $"{configuration.GetValue<string>("BaseAuthUrl")}{complement}";

            services.AddAuthorization(options =>
            {
                options.AddPolicy("email", policy => policy.Requirements.Add(new HasScopeRequirement("email", authUrl)));
            });

            return services;
        }
        public static IServiceCollection AddAPICustomAuthenticationConfig(this IServiceCollection services,
                                                                                            IWebHostEnvironment environment,
                                                                                            IConfiguration configuration)
        {
            var clientId = configuration.GetValue<string>("ClientId");
            var clientSecret = configuration.GetValue<string>("ClientSecret");
            var key = new SymmetricSecurityKey(Encoding.ASCII.GetBytes(clientSecret)) { KeyId = clientId };

            var complement = configuration.GetValue<string>("UrlComplement");
            var authUrl = $"{configuration.GetValue<string>("BaseAuthUrl")}{complement}";
            var audience = configuration.GetValue<string>("Audience");


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

        public static IServiceCollection AddMVCCustomAuthenticationConfig(this IServiceCollection services,
                                                                                    IWebHostEnvironment environment,
                                                                                    IConfiguration configuration)
        {

            var clientId = configuration.GetValue<string>("ClientId");
            var clientSecret = configuration.GetValue<string>("ClientSecret");
            var key = new SymmetricSecurityKey(Encoding.ASCII.GetBytes(clientSecret)) { KeyId = clientId };

            var complement = configuration.GetValue<string>("UrlComplement");
            var authUrl = $"{configuration.GetValue<string>("BaseAuthUrl")}{complement}";
            var audience = configuration.GetValue<string>("Audience");

            services.AddAuthentication(options =>
            {
                options.DefaultScheme = CookieAuthenticationDefaults.AuthenticationScheme;
                options.DefaultAuthenticateScheme = CookieAuthenticationDefaults.AuthenticationScheme;
                options.DefaultSignInScheme = CookieAuthenticationDefaults.AuthenticationScheme;
                options.DefaultChallengeScheme = OpenIdConnectDefaults.AuthenticationScheme;
            })
           .AddCookie(CookieAuthenticationDefaults.AuthenticationScheme, options =>
           {
               options.Cookie.Name = environment.EnvironmentName;
           })
           .AddOpenIdConnect(OpenIdConnectDefaults.AuthenticationScheme, options =>
           {
               options.Authority = authUrl;
               options.ClientId = clientId;
               options.ClientSecret = clientSecret;
               // Para o fusionAuth só vai o code
               options.ResponseType = environment.EnvironmentName == "Fusionauth" ? OpenIdConnectResponseType.Code : OpenIdConnectResponseType.CodeIdTokenToken;
               options.Scope.Clear();
               options.Scope.Add("openid");
               options.Scope.Add("profile");
               options.Scope.Add("email");
               options.RequireHttpsMetadata = false;
               options.SaveTokens = true;


               options.TokenValidationParameters.IssuerSigningKey = key;
               options.Events = SetupOpenIdConnectEvents(authUrl, audience);

           });

            return services;
        }


        /// Tentativa de correção para erro de correlationid no chrome 
        /// https://docs.microsoft.com/en-us/aspnet/core/security/samesite?view=aspnetcore-3.1
        /// https://github.com/dotnet/aspnetcore/issues/14996
        public static IServiceCollection AddMVCCustomCookiePolicyOptionsConfig(this IServiceCollection services)
        {
            JwtSecurityTokenHandler.DefaultMapInboundClaims = false;

            services.Configure<CookiePolicyOptions>(options =>
            {
                options.MinimumSameSitePolicy = SameSiteMode.Unspecified;
                options.OnAppendCookie = cookieContext =>
                    CheckSameSite(cookieContext.Context, cookieContext.CookieOptions);
                options.OnDeleteCookie = cookieContext =>
                    CheckSameSite(cookieContext.Context, cookieContext.CookieOptions);
            });
            return services;
        }

        private static OpenIdConnectEvents SetupOpenIdConnectEvents(string authUrl, string audience)
        {
            // options.CallbackPath = new PathString("/callback");
            // options.ClaimsIssuer = authUrl;
            // options.GetClaimsFromUserInfoEndpoint = true;


            var events = new OpenIdConnectEvents
            {
                OnTicketReceived = context =>
                {
                    var token = context.Properties.Items.FirstOrDefault(x => x.Key.Contains("access_token"));
                    Debug.WriteLine($"---------------------------------- Token ---------------------------------------------");
                    Debug.WriteLine(token);
                    Debug.WriteLine($"---------------------------------- Token ---------------------------------------------");
                    if (!string.IsNullOrEmpty(token.Value) && token.Value.IndexOf(".") > 0)
                    {

                        var handler = new JwtSecurityTokenHandler();
                        var userToken = handler.ReadJwtToken(token.Value);
                        //TODO: Pegar os dados do usuário do token e verificar se existe caso não cadastrar

                    }
                    return Task.CompletedTask;
                },
                OnRedirectToIdentityProvider = context =>
                {
                    if (!string.IsNullOrEmpty(audience))
                        context.ProtocolMessage.SetParameter("audience", audience);

                    return Task.CompletedTask;
                },
                OnMessageReceived = context =>
                {
                    if (!string.IsNullOrEmpty(context.ProtocolMessage.IssuerAddress))
                        context.ProtocolMessage.IssuerAddress = authUrl;
                    if (!string.IsNullOrEmpty(context.ProtocolMessage.Iss))
                        context.ProtocolMessage.Iss = authUrl;

                    return Task.CompletedTask;
                },
                OnAuthorizationCodeReceived = context =>
                {
                    if (!string.IsNullOrEmpty(context.ProtocolMessage.IssuerAddress))
                        context.ProtocolMessage.IssuerAddress = authUrl;
                    if (!string.IsNullOrEmpty(context.ProtocolMessage.Iss))
                        context.ProtocolMessage.Iss = authUrl;

                    return Task.CompletedTask;
                },
                OnTokenResponseReceived = context =>
                {
                    if (!string.IsNullOrEmpty(context.ProtocolMessage.IssuerAddress))
                        context.ProtocolMessage.IssuerAddress = authUrl;
                    if (!string.IsNullOrEmpty(context.ProtocolMessage.Iss))
                        context.ProtocolMessage.Iss = authUrl;

                    return Task.CompletedTask;
                },
                OnTokenValidated = context =>
                {
                    if (!string.IsNullOrEmpty(context.ProtocolMessage.IssuerAddress))
                        context.ProtocolMessage.IssuerAddress = authUrl;
                    if (!string.IsNullOrEmpty(context.ProtocolMessage.Iss))
                        context.ProtocolMessage.Iss = authUrl;

                    return Task.CompletedTask;
                },
                OnUserInformationReceived = context =>
                {
                    if (!string.IsNullOrEmpty(context.ProtocolMessage.IssuerAddress))
                        context.ProtocolMessage.IssuerAddress = authUrl;
                    if (!string.IsNullOrEmpty(context.ProtocolMessage.Iss))
                        context.ProtocolMessage.Iss = authUrl;

                    return Task.CompletedTask;
                },
            };

            return events;
        }





        private static void CheckSameSite(HttpContext httpContext, CookieOptions options)
        {
            if (options.SameSite == SameSiteMode.None)
            {
                var userAgent = httpContext.Request.Headers["User-Agent"].ToString();
                if (DisallowsSameSiteNone(userAgent))
                {
                    options.SameSite = SameSiteMode.Unspecified;
                }
            }
        }

        private static bool DisallowsSameSiteNone(string userAgent)
        {
            // Check if a null or empty string has been passed in, since this
            // will cause further interrogation of the useragent to fail.
            if (String.IsNullOrWhiteSpace(userAgent))
                return false;

            // Cover all iOS based browsers here. This includes:
            // - Safari on iOS 12 for iPhone, iPod Touch, iPad
            // - WkWebview on iOS 12 for iPhone, iPod Touch, iPad
            // - Chrome on iOS 12 for iPhone, iPod Touch, iPad
            // All of which are broken by SameSite=None, because they use the iOS networking
            // stack.
            if (userAgent.Contains("CPU iPhone OS 12") ||
                userAgent.Contains("iPad; CPU OS 12"))
            {
                return true;
            }

            // Cover Mac OS X based browsers that use the Mac OS networking stack. 
            // This includes:
            // - Safari on Mac OS X.
            // This does not include:
            // - Chrome on Mac OS X
            // Because they do not use the Mac OS networking stack.
            if (userAgent.Contains("Macintosh; Intel Mac OS X 10_14") &&
                userAgent.Contains("Version/") && userAgent.Contains("Safari"))
            {
                return true;
            }

            // Cover Chrome 50-69, because some versions are broken by SameSite=None, 
            // and none in this range require it.
            // Note: this covers some pre-Chromium Edge versions, 
            // but pre-Chromium Edge does not require SameSite=None.
            if (userAgent.Contains("Chrome/"))
            {
                return true;
            }

            return false;
        }
    }
}