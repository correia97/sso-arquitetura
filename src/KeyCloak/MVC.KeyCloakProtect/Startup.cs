using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.IdentityModel.Logging;
using Microsoft.IdentityModel.Protocols.OpenIdConnect;
using MVC.KeyCloakProtect.Interfaces;
using MVC.KeyCloakProtect.Services;
using Newtonsoft.Json;
using System;
using System.Diagnostics;
using System.Linq;

namespace MVC.KeyCloakProtect
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


            var complement = Configuration.GetValue<string>("UrlComplement");
            var authUrl = $"{Configuration.GetValue<string>("BaseAuthUrl")}{complement}";
            var clientId = Configuration.GetValue<string>("ClientId");
            var clientSecret = Configuration.GetValue<string>("ClientSecret");
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
                options.ResponseType = OpenIdConnectResponseType.CodeIdTokenToken;
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
                    OnTicketReceived = async context =>
                    {
                        var token = context.Properties.Items.FirstOrDefault(x => x.Key.Contains("access_token"));
                        if (!string.IsNullOrEmpty(token.Value) && token.Value.IndexOf(".")>0)
                        {
                            var paylod = token.Value.Split('.')[1];
                            var json = System.Text.ASCIIEncoding.ASCII.GetString(Convert.FromBase64String(paylod));
                            var data = JsonConvert.DeserializeObject(json);

                        }
                    },
                    OnRedirectToIdentityProvider = async context =>
                    {
                        //Debug.WriteLine($"Redirect UrI {context.ProtocolMessage.RedirectUri}");
                        //Console.WriteLine($"Redirect UrI {context.ProtocolMessage.RedirectUri}");

                        //if (context.ProtocolMessage.RedirectUri.IndexOf("8088") <= 0)
                        //    context.ProtocolMessage.RedirectUri = context.ProtocolMessage.RedirectUri.Replace(authUrl.Replace(":8088", ""), authUrl);
                    },
                };

            });


            services.AddControllersWithViews();

            services.AddScoped<IApiService, ApiService>();
            /// Tentativa de correção para erro de correlationid no chrome 
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
