using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using MVC.Interfaces;
using MVC.Models;
using System.Diagnostics;
using System.Threading.Tasks;

namespace MVC.Controllers
{
    public class HomeController : BaseController
    {
        private readonly ILogger<HomeController> _logger;
        private readonly IApiService _apiService;
        private readonly IConfiguration _configuration;
        public HomeController(ILogger<HomeController> logger, IApiService apiService, IConfiguration configuration)
        {
            _logger = logger;
            _apiService = apiService;
            _configuration = configuration;
        }

        public async Task<IActionResult> Index()
        {
            ViewBag.BaseAuthUrl = _configuration.GetValue<string>("BaseAuthUrl");
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
