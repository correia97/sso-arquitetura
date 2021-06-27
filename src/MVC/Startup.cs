using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
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
using MVC.Interfaces;
using MVC.Services;
using Newtonsoft.Json;
using System;
using System.Diagnostics;
using System.IdentityModel.Tokens.Jwt;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MVC
{
    public class Startup
    {
        public Startup(IConfiguration configuration, IWebHostEnvironment env)
        {
            Configuration = configuration;
            Environment = env;
        }

        public IConfiguration Configuration { get; }
        public IWebHostEnvironment Environment { get; }


        // This method gets called by the runtime. Use this method to add services to the container.
        public void ConfigureServices(IServiceCollection services)
        {

            IdentityModelEventSource.ShowPII = true;
            JwtSecurityTokenHandler.DefaultMapInboundClaims = false;


            var complement = Configuration.GetValue<string>("UrlComplement");
            var authUrl = $"{Configuration.GetValue<string>("BaseAuthUrl")}{complement}";
            var clientId = Configuration.GetValue<string>("ClientId");
            var clientSecret = Configuration.GetValue<string>("ClientSecret");
            var audience = Configuration.GetValue<string>("Audience");

            var key = new SymmetricSecurityKey(Encoding.ASCII.GetBytes(clientSecret)) { KeyId = clientId };

            // "b7983e28-b53a-4c40-b11a-a1bcfe0d2d34"
            services.AddAuthentication(options =>
            {
                options.DefaultScheme = CookieAuthenticationDefaults.AuthenticationScheme;
                options.DefaultAuthenticateScheme = CookieAuthenticationDefaults.AuthenticationScheme;
                options.DefaultSignInScheme = CookieAuthenticationDefaults.AuthenticationScheme;
                options.DefaultChallengeScheme = OpenIdConnectDefaults.AuthenticationScheme;
            })
            .AddCookie(CookieAuthenticationDefaults.AuthenticationScheme, options =>
            {
                options.Cookie.Name = Environment.EnvironmentName;
            })
            .AddOpenIdConnect(OpenIdConnectDefaults.AuthenticationScheme, options =>
            {
                options.Authority = authUrl;
                options.ClientId = clientId;
                options.ClientSecret = clientSecret;
                // Para o fusionAuth s� vai o code
                options.ResponseType = Environment.EnvironmentName == "Fusionauth" ? OpenIdConnectResponseType.Code : OpenIdConnectResponseType.CodeIdTokenToken;
                options.Scope.Clear();
                options.Scope.Add("openid");
                options.Scope.Add("profile");
                options.Scope.Add("email");
                options.RequireHttpsMetadata = false;
                options.SaveTokens = true;


                options.TokenValidationParameters.IssuerSigningKey = key;
                // options.CallbackPath = new PathString("/callback");
                // options.ClaimsIssuer = authUrl;
                // options.GetClaimsFromUserInfoEndpoint = true;


                options.Events = new OpenIdConnectEvents
                {
                    OnTicketReceived =  context =>
                    {
                        var token = context.Properties.Items.FirstOrDefault(x => x.Key.Contains("access_token"));
                        Debug.WriteLine($"---------------------------------- Token ---------------------------------------------");
                        Debug.WriteLine(token);
                        Debug.WriteLine($"---------------------------------- Token ---------------------------------------------");
                        if (!string.IsNullOrEmpty(token.Value) && token.Value.IndexOf(".") > 0)
                        {

                            var handler = new JwtSecurityTokenHandler();
                            var userToken = handler.ReadJwtToken(token.Value);
                            //TODO: Pegar os dados do usu�rio do token e verificar se existe caso n�o cadastrar
                           
                        }
                        return Task.CompletedTask;
                    },
                    OnRedirectToIdentityProvider = context =>
                    {
                        if (!string.IsNullOrEmpty(audience))
                            context.ProtocolMessage.SetParameter("audience", audience);

                        return Task.CompletedTask;
                    },
                    OnMessageReceived =  context =>
                    {
                        if (!string.IsNullOrEmpty(context.ProtocolMessage.IssuerAddress))
                            context.ProtocolMessage.IssuerAddress = authUrl;
                        if (!string.IsNullOrEmpty(context.ProtocolMessage.Iss))
                            context.ProtocolMessage.Iss = authUrl;

                        return Task.CompletedTask;
                    },
                    OnAuthorizationCodeReceived =  context =>
                    {
                        if (!string.IsNullOrEmpty(context.ProtocolMessage.IssuerAddress))
                            context.ProtocolMessage.IssuerAddress = authUrl;
                        if (!string.IsNullOrEmpty(context.ProtocolMessage.Iss))
                            context.ProtocolMessage.Iss = authUrl;

                        return Task.CompletedTask;
                    },
                    OnTokenResponseReceived =  context =>
                    {
                        if (!string.IsNullOrEmpty(context.ProtocolMessage.IssuerAddress))
                            context.ProtocolMessage.IssuerAddress = authUrl;
                        if (!string.IsNullOrEmpty(context.ProtocolMessage.Iss))
                            context.ProtocolMessage.Iss = authUrl;

                        return Task.CompletedTask;
                    },
                    OnTokenValidated =  context =>
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

            });


            services.AddControllersWithViews();

            services.AddScoped<IApiService, ApiService>();
            /// Tentativa de corre��o para erro de correlationid no chrome 
            /// https://docs.microsoft.com/en-us/aspnet/core/security/samesite?view=aspnetcore-3.1
            /// https://github.com/dotnet/aspnetcore/issues/14996
            services.Configure<CookiePolicyOptions>(options =>
            {
                options.MinimumSameSitePolicy = SameSiteMode.Unspecified;
                options.OnAppendCookie = cookieContext =>
                    CheckSameSite(cookieContext.Context, cookieContext.CookieOptions);
                options.OnDeleteCookie = cookieContext =>
                    CheckSameSite(cookieContext.Context, cookieContext.CookieOptions);
            });
        }

        // This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
        public void Configure(IApplicationBuilder app, IWebHostEnvironment env)
        {

            app.UseDeveloperExceptionPage();
            //if (env.IsDevelopment())
            //{
            //    app.UseDeveloperExceptionPage();
            //}
            //else
            //{
            //    app.UseExceptionHandler("/Home/Error");
            //    // The default HSTS value is 30 days. You may want to change this for production scenarios, see https://aka.ms/aspnetcore-hsts.
            //    app.UseHsts();
            //}
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
        private void CheckSameSite(HttpContext httpContext, CookieOptions options)
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
