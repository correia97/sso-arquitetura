using Cadastro.API.Interfaces;
using Domain.Entities;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using Swashbuckle.AspNetCore.Annotations;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Cadastro.API.Controllers
{
    [Authorize]
    [ApiController]
    [Route("api/[controller]")]
    public class FuncionarioController : ControllerBase
    {
        private readonly ILogger<FuncionarioController> _logger;
        private readonly IFuncionarioAppService _service;
        public FuncionarioController(ILogger<FuncionarioController> logger, IFuncionarioAppService service)
        {
            _logger = logger;
            _service = service;
        }

        [HttpGet]
        [Route("funcionario")]
        [SwaggerResponse(200, "Funcionarios localizado", typeof(IEnumerable<Funcionario>))]
        [SwaggerResponse(400, "Funcionarios não localizado")]
        public async Task<IActionResult> Get()
        {
            try
            {
                var result = await _service.ObterTodos();
                return Ok(result);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Get");
                return BadRequest(ex);
            }
        }
        [HttpGet]
        [Route("funcionario/{id:guid}")]
        [SwaggerResponse(200, "Funcionario localizado", typeof(IEnumerable<Funcionario>))]
        [SwaggerResponse(400, "Funcionario não localizado")]
        public async Task<IActionResult> Get(Guid id)
        {
            try
            {
                var result = await _service.ObterPorId(id);
                return Ok(result);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Get");
                return BadRequest(ex);
            }

        }

        [HttpPost]
        [Route("funcionario")]
        [SwaggerResponse(200, "Funcionario recebido", typeof(bool))]
        [SwaggerResponse(400, "Funcionario não recebido")]
        public IActionResult Post([FromBody] Funcionario funcionario)
        {
            try
            {
                var result =  _service.Cadastrar(funcionario);
                return Ok(result);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Post");
                return BadRequest(ex);
            }

        }

        [HttpPatch]
        [Route("funcionario")]
        [SwaggerResponse(200, "Funcionario recebido", typeof(bool))]
        [SwaggerResponse(400, "Funcionario não recebido")]
        public IActionResult Patch([FromBody] Funcionario funcionario)
        {
            try
            {
                var result = _service.Atualizar(funcionario, string.Empty);
                return Ok(result);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Patch");
                return BadRequest(ex);
            }
        }
    }
}
