using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Authentication.OpenIdConnect;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.IdentityModel.Logging;
using Microsoft.IdentityModel.Protocols.OpenIdConnect;
using Microsoft.IdentityModel.Tokens;
using MVC.Interfaces;
using MVC.Services;
using System;
using System.Diagnostics;
using System.IdentityModel.Tokens.Jwt;
using System.Linq;
using System.Text;
using System.Threading.Tasks;



void CheckSameSite(HttpContext httpContext, CookieOptions options)
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

bool DisallowsSameSiteNone(string userAgent)
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

IdentityModelEventSource.ShowPII = true;
JwtSecurityTokenHandler.DefaultMapInboundClaims = false;
System.Net.ServicePointManager.ServerCertificateValidationCallback +=
(sender, certificate, chain, sslPolicyErrors) => true;

var builder = WebApplication.CreateBuilder(args);

var complement = builder.Configuration.GetValue<string>("UrlComplement");
var authUrl = $"{builder.Configuration.GetValue<string>("BaseAuthUrl")}{complement}";
var clientId = builder.Configuration.GetValue<string>("ClientId");
var clientSecret = builder.Configuration.GetValue<string>("ClientSecret");
var audience = builder.Configuration.GetValue<string>("Audience");

var key = new SymmetricSecurityKey(Encoding.ASCII.GetBytes(clientSecret)) { KeyId = clientId };

builder.Services.AddAuthentication(options =>
{
    options.DefaultScheme = CookieAuthenticationDefaults.AuthenticationScheme;
    options.DefaultAuthenticateScheme = CookieAuthenticationDefaults.AuthenticationScheme;
    options.DefaultSignInScheme = CookieAuthenticationDefaults.AuthenticationScheme;
    options.DefaultChallengeScheme = OpenIdConnectDefaults.AuthenticationScheme;
})
           .AddCookie(CookieAuthenticationDefaults.AuthenticationScheme, options =>
           {
               options.Cookie.Name = builder.Environment.EnvironmentName;
           })
           .AddOpenIdConnect(OpenIdConnectDefaults.AuthenticationScheme, options =>
           {
               options.Authority = authUrl;
               options.ClientId = clientId;
               options.ClientSecret = clientSecret;
               // Para o fusionAuth só vai o code
               options.ResponseType = builder.Environment.EnvironmentName == "Fusionauth" ? OpenIdConnectResponseType.Code : OpenIdConnectResponseType.CodeIdTokenToken;
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

           });



builder.Services.AddControllersWithViews();

builder.Services.AddScoped<IApiService, ApiService>();
/// Tentativa de correção para erro de correlationid no chrome 
/// https://docs.microsoft.com/en-us/aspnet/core/security/samesite?view=aspnetcore-3.1
/// https://github.com/dotnet/aspnetcore/issues/14996
builder.Services.Configure<CookiePolicyOptions>(options =>
{
    options.MinimumSameSitePolicy = SameSiteMode.Unspecified;
    options.OnAppendCookie = cookieContext =>
        CheckSameSite(cookieContext.Context, cookieContext.CookieOptions);
    options.OnDeleteCookie = cookieContext =>
        CheckSameSite(cookieContext.Context, cookieContext.CookieOptions);
});


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