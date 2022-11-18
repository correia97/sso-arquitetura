using Cadastro.MVC.Interfaces;
using Cadastro.MVC.Models.Request;
using Microsoft.AspNetCore.Mvc;
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
        public FuncionarioController(ILogger<FuncionarioController> logger, IFuncionarioService apiService)
        {
            _logger = logger;
            _apiService = apiService;
        }
        public async Task<ActionResult> Index([FromQuery] int pagina = 1, [FromQuery] int qtdItens = 10)
        {
            if (!User.Identity.IsAuthenticated)
                return RedirectToAction("Index", "Home");

            await GetTokens();

            await _apiService.CadastrarFuncionario(new FuncionarioRequest
            {
                Ativo = true,
                Email = User.Claims.First(x => x.Type == ClaimTypes.Email).Value,
                Nome = User.Claims.First(x => x.Type == ClaimTypes.GivenName).Value,
                SobreNome = User.Claims.First(x => x.Type == ClaimTypes.Surname).Value,
                UserId = this.UserId.ToString()
            }, this.AccessToken);

            if (IsAdmin)
            {
                pagina = pagina > 0 ? pagina - 1 : 0;
                var funcionarios = await _apiService.ListarFuncionarios(this.AccessToken, pagina, qtdItens);
                if (funcionarios.Sucesso)
                {
                    ViewBag.ItensPorPagina = qtdItens;
                    ViewBag.paginaAtual = pagina + 1;
                    return View(funcionarios);
                }

                return RedirectToAction("Error", "Home");
            }

            return RedirectToAction("Edit", "Funcionario", new { id = this.UserId });
        }


        public ActionResult Create()
        {
            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Create([FromForm] FuncionarioRequest request)
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

        public async Task<ActionResult> Edit(Guid id)
        {
            if (!User.Identity.IsAuthenticated)
                return RedirectToAction("Index", "Home");

            await GetTokens();
            var funcionario = await _apiService.RecuperarFuncionario(id != Guid.Empty ? id : this.UserId, this.AccessToken);
            if (funcionario.Sucesso)
            {
                return View(funcionario.Data);
            }
            return RedirectToAction("Error", "Home");
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<ActionResult> Edit(Guid id, [FromForm] FuncionarioRequest request)
        {
            try
            {
                if (!User.Identity.IsAuthenticated)
                    return RedirectToAction("Index", "Home");

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
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Erro ao atualizar usuário {id}");
                return RedirectToAction("Error", "Home");
            }
        }

        public async Task<ActionResult> Delete(Guid id)
        {
            if (!User.Identity.IsAuthenticated)
                return RedirectToAction("Index", "Home");

            await GetTokens();
            var funcionario = await _apiService.RecuperarFuncionario(id != Guid.Empty ? id : this.UserId, this.AccessToken);
            if (funcionario.Sucesso)
            {
                return View(funcionario.Data);
            }
            return RedirectToAction("Error", "Home");
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<ActionResult> ConfirmDelete([FromForm] Guid userId)
        {

            if (!User.Identity.IsAuthenticated)
                return RedirectToAction("Index", "Home");

            await GetTokens();
            var funcionario = await _apiService.RemoverFuncionario(userId != Guid.Empty ? userId : this.UserId, this.AccessToken);
            if (funcionario.Sucesso)
            {
                return RedirectToAction("Index", "Funcionario");
            }
            return RedirectToAction("Error", "Home");
        }
    }
}
