using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using MVC.KeyCloackProtect.Interfaces;
using MVC.KeyCloackProtect.Models;
using System.Diagnostics;
using System.Threading.Tasks;

namespace MVC.KeyCloackProtect.Controllers
{
    public class HomeController : BaseController
    {
        private readonly ILogger<HomeController> _logger;
        private readonly IApiService _apiService;
        public HomeController(ILogger<HomeController> logger, IApiService apiService)
        {
            _logger = logger;
            _apiService = apiService;
        }

        public async Task<IActionResult> Index()
        {
            if (User.Identity.IsAuthenticated)
            {
                var accessToken = await HttpContext.GetTokenAsync("access_token");
                var IdToken = await HttpContext.GetTokenAsync("id_token");
                var expire = await HttpContext.GetTokenAsync("expires_at");
                var forecast = await _apiService.GetWeatherForecast(accessToken);
                ViewBag.forecast = forecast;
            }

            return View();
        }
        public async Task Login(string returnUrl = "/")
        {
            await HttpContext.ChallengeAsync("auth", new AuthenticationProperties() { RedirectUri = returnUrl });
        }

        [Authorize]
        public IActionResult Privacy()
        {
            return View();
        }

        [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
        public IActionResult Error()
        {
            return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
        }
    }
}
