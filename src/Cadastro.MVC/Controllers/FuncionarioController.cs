using Cadastro.MVC.Interfaces;
using Cadastro.MVC.Models.Request;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using MVC.Controllers;
using MVC.Interfaces;
using System;
using System.Linq;
using System.Security.Claims;
using System.Threading.Tasks;

namespace Cadastro.MVC.Controllers
{
    public class FuncionarioController : BaseController
    {
        private readonly ILogger<FuncionarioController> _logger;
        private readonly IFuncionarioService _apiService;
        private readonly IConfiguration _configuration;

        public FuncionarioController(ILogger<FuncionarioController> logger, IFuncionarioService apiService, IConfiguration configuration)
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
                await GetTokens();
                var funcionario = await _apiService.RecuperarFuncionario(this.UserId, this.AccessToken);
                if (!funcionario.Sucesso)
                {
                    var result = await _apiService.CadastrarFuncionario(new FuncionarioRequest
                    {
                        Ativo = true,
                        Email = User.Claims.First(x => x.Type == ClaimTypes.Email).Value,
                        Nome = User.Claims.First(x => x.Type == ClaimTypes.GivenName).Value,
                        SobreNome = User.Claims.First(x => x.Type == ClaimTypes.Surname).Value,
                        UserId = this.UserId.ToString()
                    }, this.AccessToken);
                    if (!result.Sucesso)
                        return RedirectToPage("Home");
                }
                return RedirectToAction("Edit", new { id = this.UserId });
            }
            return RedirectToPage("Home");
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
        public async Task<ActionResult> Edit(Guid id)
        {
            if (User.Identity.IsAuthenticated)
            {
                await GetTokens();
                var funcionario = await _apiService.RecuperarFuncionario(id != Guid.Empty ? id : this.UserId, this.AccessToken);
                if (!funcionario.Sucesso)
                {
                    return View(funcionario.Data);
                }
            }
            return RedirectToPage("Home");
        }

        // POST: FuncionarioController/Edit/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<ActionResult> Edit(Guid id, [FromForm] FuncionarioRequest request)
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
