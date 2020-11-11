using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using MVC.KeyCloackProtect.Models;
using System;
using System.Diagnostics;
using System.Globalization;
using System.Linq;
using System.Threading.Tasks;

namespace MVC.KeyCloackProtect.Controllers
{
    public class HomeController : Controller
    {
        private readonly ILogger<HomeController> _logger;

        public HomeController(ILogger<HomeController> logger)
        {
            _logger = logger;
        }

        public async Task<IActionResult> Index()
        {
            if (User.Identity.IsAuthenticated)
            {
                Debug.WriteLine("------------------------------------------------------------");
                Debug.WriteLine("------------------------------------------------------------");
                Debug.WriteLine("------------------------------------------------------------");
                Debug.WriteLine("------------------------------------------------------------");
                Debug.WriteLine("User Claims");
                foreach (var item in User.Claims)
                {
                    Debug.WriteLine($"{item.Type}:{item.Value}");
                }
                Debug.WriteLine("------------------------------------------------------------");
                Debug.WriteLine("------------------------------------------------------------");
                Debug.WriteLine("------------------------------------------------------------");
                Debug.WriteLine("------------------------------------------------------------");
                Debug.WriteLine("------------------------------------------------------------");
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
