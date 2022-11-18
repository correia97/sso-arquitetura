using Cadastro.Domain.Entities;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Authentication.OpenIdConnect;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.IdentityModel.Logging;
using Microsoft.IdentityModel.Protocols;
using Microsoft.IdentityModel.Protocols.OpenIdConnect;
using Microsoft.IdentityModel.Tokens;
using System.Diagnostics;
using System.Diagnostics.CodeAnalysis;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text.Json;

namespace Cadastro.Configuracoes
{
    [ExcludeFromCodeCoverage]
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
            var complement = configuration.GetValue<string>("UrlComplement");
            var authUrl = $"{configuration.GetValue<string>("BaseAuthUrl")}{complement}";
            var audience = configuration.GetValue<string>("Audience");

            services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
                    .AddJwtBearer(options =>
                    {
                        options.Authority = authUrl;
                        if (!string.IsNullOrEmpty(audience))
                            options.Audience = audience;
                        options.RequireHttpsMetadata = false;
                        options.SaveToken = true;
                        options.TokenValidationParameters.NameClaimType = ClaimTypes.NameIdentifier;
                        options.TokenValidationParameters.ValidateIssuerSigningKey = false;
                        options.TokenValidationParameters.ValidateIssuer = false;
                        options.Events = new JwtBearerEvents
                        {
                            OnTokenValidated = context =>
                            {
                                var token = (JwtSecurityToken)context.SecurityToken;
                                var payload = JsonSerializer.Deserialize<TokenPayload>(token.Payload.SerializeToJson());
                                context.Principal.AddIdentities(FillToken(payload));
                                return Task.CompletedTask;
                            },
                            OnAuthenticationFailed = context =>
                            {
                                context.NoResult();
                                context.Response.StatusCode = 401;
                                context.Response.ContentType = "text/plain";
                                if (!environment.EnvironmentName.ToUpper().Contains("PROD"))
                                {
                                    return context.Response.WriteAsync(context.Exception.ToString());
                                }
                                return context.Response.WriteAsync("An error occured processing your authentication.");
                            },
                        };
                    });

            return services;
        }

        private static List<ClaimsIdentity> FillToken(TokenPayload payload)
        {
            var claims = new List<Claim>();
            if (payload != null && !string.IsNullOrEmpty(payload.given_name))
            {
                claims.Add(new Claim(ClaimTypes.GivenName, payload.given_name));
                claims.Add(new Claim(ClaimTypes.Name, payload.name));
                claims.Add(new Claim(ClaimTypes.Email, payload.email));
                claims.Add(new Claim(ClaimTypes.Surname, payload.family_name));
                claims.Add(new Claim("userId", payload.sub));

                AddClaimFromRoleList(claims, payload.group);
                AddClaimFromRoleList(claims, payload.realm_access?.roles);
                AddClaimFromRoleList(claims, payload.resource_access?.account?.roles);
            }
            var identity = new ClaimsIdentity(claims);
            return new List<ClaimsIdentity> { identity };
        }

        private static void AddClaimFromRoleList(List<Claim> claims, List<string> roles)
        {
            if (roles != null)
                foreach (var item in roles)
                {
                    claims.Add(new Claim(ClaimTypes.Role, item));
                }
        }

        public static IServiceCollection AddMVCCustomAuthenticationConfig(this IServiceCollection services,
                                                                                    IWebHostEnvironment environment,
                                                                                    IConfiguration configuration)
        {
            JwtSecurityTokenHandler.DefaultMapInboundClaims = false;
            var clientId = configuration.GetValue<string>("ClientId");
            var clientSecret = configuration.GetValue<string>("ClientSecret");
            var complement = configuration.GetValue<string>("UrlComplement");
            var baseUrl = configuration.GetValue<string>("BaseAuthUrl");
            var metaDataUrl = configuration.GetValue<string>("MetaDataUrl");
            var authUrl = $"{baseUrl}{complement}";

            IdentityModelEventSource.ShowPII = true;

            _ = services.AddAuthentication(options =>
            {
                options.DefaultScheme = OpenIdConnectDefaults.AuthenticationScheme;
                options.DefaultSignInScheme = OpenIdConnectDefaults.AuthenticationScheme;
                options.DefaultChallengeScheme = OpenIdConnectDefaults.AuthenticationScheme;
            })
           .AddCookie(CookieAuthenticationDefaults.AuthenticationScheme, options =>
           {
               options.Cookie.Name = $"ambiente_{environment.EnvironmentName}";
           })
           .AddOpenIdConnect(OpenIdConnectDefaults.AuthenticationScheme, async options =>
           {
               options.SignInScheme = CookieAuthenticationDefaults.AuthenticationScheme;
               options.Authority = authUrl;
               options.ClientId = clientId;
               options.ClientSecret = clientSecret;
               options.ClaimsIssuer = authUrl;
               options.MetadataAddress = $"{metaDataUrl}{complement}/.well-known/openid-configuration";

               // Para o fusionAuth só vai o code
               options.ResponseType = environment.EnvironmentName == "Fusionauth" ? OpenIdConnectResponseType.Code : OpenIdConnectResponseType.CodeIdTokenToken;
               options.Scope.Clear();
               options.Scope.Add("openid");
               options.Scope.Add("profile");
               options.Scope.Add("email");
               options.RequireHttpsMetadata = false;
               options.SaveTokens = true;

               options.GetClaimsFromUserInfoEndpoint = false;
               options.NonceCookie.SameSite = SameSiteMode.Unspecified;
               options.CorrelationCookie.SameSite = SameSiteMode.Unspecified;

               var configManager = new ConfigurationManager<OpenIdConnectConfiguration>(
                   $"{metaDataUrl}{complement}/.well-known/openid-configuration",
                   new OpenIdConnectConfigurationRetriever(),
                   new HttpDocumentRetriever()
                   {
                       RequireHttps = false
                   });

               var openidconfig = await configManager.GetConfigurationAsync();

               options.TokenValidationParameters = new TokenValidationParameters
               {
                   ValidateIssuer = true,
                   ValidIssuer = authUrl,
                   ValidAudience = clientId,
                   IssuerSigningKeys = openidconfig.SigningKeys,
               };

               options.Configuration = openidconfig;

               options.Events = SetupOpenIdConnectEvents();

           });

            return services;
        }


        /// Tentativa de correção para erro de correlationid no chrome 
        /// https://docs.microsoft.com/pt-br/aspnet/core/security/samesite?view=aspnetcore-6.0
        /// https://github.com/dotnet/aspnetcore/issues/14996
        public static IServiceCollection AddMVCCustomCookiePolicyOptionsConfig(this IServiceCollection services)
        {
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

        private static OpenIdConnectEvents SetupOpenIdConnectEvents()
        {
            var events = new OpenIdConnectEvents
            {
                OnTokenValidated = context =>
                {
                    var tokenJwt = context.SecurityToken;
                    if (tokenJwt != null && !string.IsNullOrEmpty(tokenJwt.RawPayload))
                    {
                        var payload = JsonSerializer.Deserialize<TokenPayload>(tokenJwt.Payload.SerializeToJson());
                        context.Principal.AddIdentities(FillToken(payload));
                    }
                    return Task.CompletedTask;
                },
                OnMessageReceived = context =>
                {
                    return Task.CompletedTask;
                },
                OnAuthorizationCodeReceived = context =>
                {
                    return Task.CompletedTask;
                },
                OnTicketReceived = context =>
                {
                    return Task.CompletedTask;
                },
                OnTokenResponseReceived = context =>
                {
                    return Task.CompletedTask;
                },
                OnUserInformationReceived = context =>
                {
                    return Task.CompletedTask;
                },
                OnAuthenticationFailed = context =>
                {
                    return Task.CompletedTask;
                },
                OnAccessDenied = context =>
                {
                    return Task.CompletedTask;
                },
                OnRemoteFailure = context =>
                {
                    return Task.CompletedTask;
                }
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

        public static bool DisallowsSameSiteNone(string userAgent)
        {

            // Check if a null or empty string has been passed in, since this
            // will cause further interrogation of the useragent to fail.
            if (string.IsNullOrWhiteSpace(userAgent))
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
                Debug.WriteLine($" User Ahent {userAgent} Return true");
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
                Debug.WriteLine($" User Ahent {userAgent} Return true");
                return true;
            }

            // Cover Chrome 50-69, because some versions are broken by SameSite=None, 
            // and none in this range require it.
            // Note: this covers some pre-Chromium Edge versions, 
            // but pre-Chromium Edge does not require SameSite=None.
            if (userAgent.Contains("Chrome/5") || userAgent.Contains("Chrome/6") || userAgent.Contains("Chrome/10"))
            {
                Debug.WriteLine($" User Ahent {userAgent} Return true");
                return true;
            }

            Debug.WriteLine($" User Ahent {userAgent} Return false");
            return false;
        }
    }
}