using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using MVC.Interfaces;
using MVC.Models;
using System;
using System.Diagnostics;
using System.Threading.Tasks;

namespace MVC.Controllers
{
    public class HomeController : BaseController
    {
        private readonly ILogger<HomeController> _logger;
        private readonly IWeatherForecastService _apiService;
        private readonly IConfiguration _configuration;
        public HomeController(ILogger<HomeController> logger, IWeatherForecastService apiService, IConfiguration configuration)
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
                var forecast = await _apiService.GetWeatherForecast(accessToken);
                ViewBag.forecast = forecast;
            }

            return View();
        }
        public async Task Login(string returnUrl = "/")
        {
            try
            {
                await HttpContext.ChallengeAsync("auth", new AuthenticationProperties() { RedirectUri = returnUrl });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Login error");
                throw;
            }
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
