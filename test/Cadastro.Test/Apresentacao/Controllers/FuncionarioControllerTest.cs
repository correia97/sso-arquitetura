using Cadastro.API.Controllers;
using Cadastro.API.Interfaces;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Xunit;

namespace Cadastro.Test.Apresentacao.Controllers
{
    public class FuncionarioControllerTest
    {

        private Mock<ILogger<FuncionarioController>> _logger;
        private Mock<IFuncionarioAppService> _service;
        public FuncionarioControllerTest()
        {
            _logger = new Mock<ILogger<FuncionarioController>>(); 
            _service = new Mock<IFuncionarioAppService>();
        }

        [Fact]
        public async Task Get_Test()
        {
            var controller = new FuncionarioController(_logger.Object, _service.Object);

            var result = await controller.Get();


        }
    }
}
