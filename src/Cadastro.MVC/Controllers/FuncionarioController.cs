using Cadastro.MVC.Models.Request;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using MVC.Controllers;
using MVC.Interfaces;
using System.Threading.Tasks;

namespace Cadastro.MVC.Controllers
{
    public class FuncionarioController : BaseController
    {
        private readonly ILogger<FuncionarioController> _logger;
        private readonly IWeatherForecastService _apiService;
        private readonly IConfiguration _configuration;

        public FuncionarioController(ILogger<FuncionarioController> logger, IWeatherForecastService apiService, IConfiguration configuration)
        {
            _logger = logger;
            _apiService = apiService;
            _configuration = configuration;
        }

        // GET: FuncionarioController
        public async Task<ActionResult> Index()
        {
            if (User.Identity.IsAuthenticated)
            {
                var accessToken = await HttpContext.GetTokenAsync("access_token");
                var forecast = await _apiService.GetWeatherForecast(accessToken);
                ViewBag.forecast = forecast;
            }

            return View();
        }


        // GET: FuncionarioController/Create
        public async Task<ActionResult> Create()
        {
            return View();
        }

        // POST: FuncionarioController/Create
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<ActionResult> Create([FromForm] FuncionarioRequest request)
        {
            try
            {
                return RedirectToAction(nameof(Index));
            }
            catch
            {
                return View();
            }
        }

        // GET: FuncionarioController/Edit/5
        public async Task<ActionResult> Edit(int id)
        {
            return View();
        }

        // POST: FuncionarioController/Edit/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<ActionResult> Edit(int id, [FromForm] FuncionarioRequest request)
        {
            try
            {
                return RedirectToAction(nameof(Index));
            }
            catch
            {
                return View();
            }
        }

        // GET: FuncionarioController/Delete/5
        public async Task<ActionResult> Delete(int id)
        {
            return View();
        }

        // POST: FuncionarioController/Delete/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<ActionResult> ConfirmDelete(int id)
        {
            try
            {
                return RedirectToAction(nameof(Index));
            }
            catch
            {
                return View();
            }
        }
    }
}
