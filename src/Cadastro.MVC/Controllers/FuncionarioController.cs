using Cadastro.MVC.Interfaces;
using Cadastro.MVC.Models.Request;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using MVC.Controllers;
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
                        return RedirectToAction("Index", "Home");
                }
                return View("Edit", funcionario.Data);
            }
            return RedirectToAction("Index", "Home");
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
                if (funcionario.Sucesso)
                {
                    return View(funcionario.Data);
                }
            }
            return RedirectToAction("Index", "Home");
        }

        // POST: FuncionarioController/Edit/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<ActionResult> Edit(Guid id, [FromForm] FuncionarioRequest request)
        {
            try
            {
                if (User.Identity.IsAuthenticated)
                {
                    if (Request.Form.Any(x => x.Key == "telefones.Id") && !string.IsNullOrEmpty(Request.Form["telefones.Id"].ToString()))
                    {
                        var ids = Request.Form["telefones.Id"].ToString().Split(',');
                        var ddi = Request.Form["telefones.DDI"].ToString().Split(',');
                        var telefone = Request.Form["telefones.Telefone"].ToString().Split(',');
                        for (int i = 0; i < telefone.Length; i++)
                        {
                            request.Telefones.Add(new TelefoneRequest
                            {
                                DDI = ddi[i],
                                Telefone = telefone[i],
                                Id = int.Parse(ids[i])
                            });
                        }
                    }

                    await GetTokens();
                    var funcionario = await _apiService.AtualizarFuncionario(request, this.AccessToken);
                    if (!funcionario.Sucesso)
                    {
                        ViewBag.erro = funcionario.Erro;
                    }
                    return RedirectToAction("Edit", "Funcionario", new { id });
                }
                return RedirectToAction("Index", "Home");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Erro ao atualizar usuário {id}");
                return View(request);
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
