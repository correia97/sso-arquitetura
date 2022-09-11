using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Authentication.OpenIdConnect;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using System.Threading.Tasks;

namespace MVC.Controllers
{
    public class AccountController : BaseController
    {
        private readonly ILogger<AccountController> _logger;
        public AccountController(ILogger<AccountController> logger)
        {
            _logger = logger;
        }
        public async Task Login(string returnUrl = "/")
        {
            try
            {
                await HttpContext.ChallengeAsync(OpenIdConnectDefaults.AuthenticationScheme, new AuthenticationProperties() { RedirectUri = returnUrl });

            }
            catch (System.Exception ex)
            {
                _logger.LogError(ex, "Login");
                throw;
            }
        }

        [Authorize]
        public async Task Logout()
        {
            await HttpContext.SignOutAsync(OpenIdConnectDefaults.AuthenticationScheme, new AuthenticationProperties
            {
                RedirectUri = Url.Action("Index", "Home")
            });
            await HttpContext.SignOutAsync(CookieAuthenticationDefaults.AuthenticationScheme);
        }


        [Authorize]
        public IActionResult Claims()
        {
            return View();
        }

        public IActionResult AccessDenied()
        {
            return View();
        }
    }
}
