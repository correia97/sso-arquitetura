using Cadastro.API.Controllers.V1;
using Cadastro.API.Interfaces;
using Cadastro.API.Models.Request;
using Domain.Entities;
using Domain.ValueObject;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using System.Linq;
using System.Runtime.ConstrainedExecution;
using System.Security.Cryptography;
using Xunit;
using Xunit.Abstractions;

namespace Cadastro.Test.Apresentacao.Controllers
{
    public class FuncionarioControllerTest
    {

        private Mock<ILogger<FuncionarioController>> _logger;
        private Mock<IFuncionarioAppService> _service;
        public readonly Faker _faker;
        private ITestOutputHelper _outputHelper { get; }

        public FuncionarioControllerTest(ITestOutputHelper outputHelper)
        {
            _logger = new Mock<ILogger<FuncionarioController>>();
            _service = new Mock<IFuncionarioAppService>();
            _faker = new Faker("pt_BR");
            _outputHelper = outputHelper;
        }

        [Fact]
        public async Task Get_Success_Test()
        {
            _service.Setup(x => x.ObterTodos()).ReturnsAsync(new List<Funcionario>());
            var controller = new FuncionarioController(_logger.Object, _service.Object);

            var result = await controller.Get() as OkObjectResult;

            _service.Verify(x => x.ObterTodos(), Times.Once);
            result.StatusCode.Should().Be(200);
        }

        [Fact]
        public async Task Get_Fail_Test()
        {
            _service.Setup(x => x.ObterTodos()).Throws(new Exception());
            var controller = new FuncionarioController(_logger.Object, _service.Object);

            var result = await controller.Get() as BadRequestObjectResult;

            _service.Verify(x => x.ObterTodos(), Times.Once);
            result.StatusCode.Should().Be(400);
        }

        [Fact]
        public async Task GetById_Success_Test()
        {
            var person = _faker.Person;
            var tels = new List<Telefone> {
                new Telefone("+55","11","90000-0000")
            };
            var endereco = new Endereco("Rua", 10, "00000-000", "apto", "bairro", "cidade", "sp");
            var funcionario = new Funcionario("xxxxxx", "matricular", "cargo",
                new Nome(person.FirstName, person.LastName),
                new DataNascimento(new System.DateTime(1987, 08, 14)),
                new Email(person.Email), tels, endereco, endereco);

            _service.Setup(x => x.ObterPorId(It.IsAny<Guid>())).ReturnsAsync(funcionario);
            var controller = new FuncionarioController(_logger.Object, _service.Object);

            var result = await controller.Get(Guid.NewGuid()) as OkObjectResult;

            _service.Verify(x => x.ObterPorId(It.IsAny<Guid>()), Times.Once);
            result.StatusCode.Should().Be(200);
        }

        [Fact]
        public async Task GetById_Fail_Test()
        {
            _service.Setup(x => x.ObterPorId(It.IsAny<Guid>())).Throws(new Exception());
            var controller = new FuncionarioController(_logger.Object, _service.Object);

            var result = await controller.Get(Guid.NewGuid()) as BadRequestObjectResult;

            _service.Verify(x => x.ObterPorId(It.IsAny<Guid>()), Times.Once);
            result.StatusCode.Should().Be(400);
        }


        [Fact]
        public void Post_Success_Test()
        {
            var person = _faker.Person;
            var tels = new List<Telefone> {
                new Telefone("+55","11","90000-0000")
            };
            var endereco = new Endereco("Rua", 10, "00000-000", "apto", "bairro", "cidade", "sp");
            var funcionario = new Funcionario("xxxxxx", "matricular", "cargo",
                new Nome(person.FirstName, person.LastName),
                new DataNascimento(new System.DateTime(1987, 08, 14)),
                new Email(person.Email), tels, endereco, endereco);

            var funcionarioReq = new FuncionarioRequest()
            {
                Ativo = funcionario.Ativo,
                Cargo = funcionario.Cargo,
                DataNascimento = funcionario.DataNascimento.Date,
                Email = funcionario.Email.ToString(),
                Matricula = funcionario.Matricula ,
                Nome = funcionario.Nome.PrimeiroNome,
                SobreNome = funcionario.Nome.SobreNome,
                UserId = funcionario.UserId ,
                Telefones = funcionario.Telefones.Select(x => new TelefoneRequest
                {
                    DDI = x.DDI,
                    Telefone = $"{x.DDD}{x.NumeroTelefone}"
                }).ToList(),
                EnderecoResidencial = new EnderecoRequest
                {
                    Rua = funcionario.EnderecoResidencial?.Rua,
                    Numero = funcionario.EnderecoResidencial?.Numero,
                    CEP = funcionario.EnderecoResidencial?.CEP,
                    Complemento = funcionario.EnderecoResidencial?.Complemento,
                    Bairro = funcionario.EnderecoResidencial?.Bairro,
                    Cidade = funcionario.EnderecoResidencial?.Cidade,
                    UF = funcionario.EnderecoResidencial?.UF,
                },
                EnderecoComercial = new EnderecoRequest
                {
                    Rua = funcionario.EnderecoComercial?.Rua,
                    Numero = funcionario.EnderecoComercial?.Numero,
                    CEP = funcionario.EnderecoComercial?.CEP,
                    Complemento = funcionario.EnderecoComercial?.Complemento,
                    Bairro = funcionario.EnderecoComercial?.Bairro,
                    Cidade = funcionario.EnderecoComercial?.Cidade,
                    UF = funcionario.EnderecoComercial?.UF,
                }
            };

            _service.Setup(x => x.Cadastrar(It.IsAny<Funcionario>())).Returns(true);
            var controller = new FuncionarioController(_logger.Object, _service.Object);

            var result = controller.Post(funcionarioReq) as OkObjectResult;

            _service.Verify(x => x.Cadastrar(It.IsAny<Funcionario>()), Times.Once);
            result.StatusCode.Should().Be(200);
        }

        [Fact]
        public void Post_Fail_Test()
        {
            var person = _faker.Person;
            var tels = new List<Telefone> {
                new Telefone("+55","11","90000-0000")
            };
            var endereco = new Endereco("Rua", 10, "00000-000", "apto", "bairro", "cidade", "sp");
            var funcionario = new Funcionario("xxxxxx", "matricular", "cargo",
                new Nome(person.FirstName, person.LastName),
                new DataNascimento(new System.DateTime(1987, 08, 14)),
                new Email(person.Email), tels, endereco, endereco);

            var funcionarioReq = new FuncionarioRequest()
            {
                Ativo = funcionario.Ativo,
                Cargo = funcionario.Cargo,
                DataNascimento = funcionario.DataNascimento.Date,
                Email = funcionario.Email.ToString(),
                Matricula = funcionario.Matricula,
                Nome = funcionario.Nome.PrimeiroNome,
                SobreNome = funcionario.Nome.SobreNome,
                UserId = funcionario.UserId,
                Telefones = funcionario.Telefones.Select(x => new TelefoneRequest
                {
                    DDI = x.DDI,
                    Telefone = $"{x.DDD}{x.NumeroTelefone}"
                }).ToList(),
                EnderecoResidencial = new EnderecoRequest
                {
                    Rua = funcionario.EnderecoResidencial?.Rua,
                    Numero = funcionario.EnderecoResidencial?.Numero,
                    CEP = funcionario.EnderecoResidencial?.CEP,
                    Complemento = funcionario.EnderecoResidencial?.Complemento,
                    Bairro = funcionario.EnderecoResidencial?.Bairro,
                    Cidade = funcionario.EnderecoResidencial?.Cidade,
                    UF = funcionario.EnderecoResidencial?.UF,
                },
                EnderecoComercial = new EnderecoRequest
                {
                    Rua = funcionario.EnderecoComercial?.Rua,
                    Numero = funcionario.EnderecoComercial?.Numero,
                    CEP = funcionario.EnderecoComercial?.CEP,
                    Complemento = funcionario.EnderecoComercial?.Complemento,
                    Bairro = funcionario.EnderecoComercial?.Bairro,
                    Cidade = funcionario.EnderecoComercial?.Cidade,
                    UF = funcionario.EnderecoComercial?.UF,
                }
            };

            _service.Setup(x => x.Cadastrar(It.IsAny<Funcionario>())).Throws(new Exception());
            var controller = new FuncionarioController(_logger.Object, _service.Object);

            var result = controller.Post(funcionarioReq) as BadRequestObjectResult;

            _service.Verify(x => x.Cadastrar(It.IsAny<Funcionario>()), Times.Once);
            result.StatusCode.Should().Be(400);
        }

        [Fact]
        public void Patch_Success_Test()
        {
            var person = _faker.Person;
            var tels = new List<Telefone> {
                new Telefone("+55","11","90000-0000")
            };
            var endereco = new Endereco("Rua", 10, "00000-000", "apto", "bairro", "cidade", "sp");
            var funcionario = new Funcionario("xxxxxx", "matricular", "cargo",
                new Nome(person.FirstName, person.LastName),
                new DataNascimento(new System.DateTime(1987, 08, 14)),
                new Email(person.Email), tels, endereco, endereco);


            var funcionarioReq = new FuncionarioRequest()
            {
                Ativo = funcionario.Ativo,
                Cargo = funcionario.Cargo,
                DataNascimento = funcionario.DataNascimento.Date,
                Email = funcionario.Email.ToString(),
                Matricula = funcionario.Matricula,
                Nome = funcionario.Nome.PrimeiroNome,
                SobreNome = funcionario.Nome.SobreNome,
                UserId = funcionario.UserId,
                Telefones = funcionario.Telefones.Select(x => new TelefoneRequest
                {
                    DDI = x.DDI,
                    Telefone = $"{x.DDD}{x.NumeroTelefone}"
                }).ToList(),
                EnderecoResidencial = new EnderecoRequest
                {
                    Rua = funcionario.EnderecoResidencial?.Rua,
                    Numero = funcionario.EnderecoResidencial?.Numero,
                    CEP = funcionario.EnderecoResidencial?.CEP,
                    Complemento = funcionario.EnderecoResidencial?.Complemento,
                    Bairro = funcionario.EnderecoResidencial?.Bairro,
                    Cidade = funcionario.EnderecoResidencial?.Cidade,
                    UF = funcionario.EnderecoResidencial?.UF,
                },
                EnderecoComercial = new EnderecoRequest
                {
                    Rua = funcionario.EnderecoComercial?.Rua,
                    Numero = funcionario.EnderecoComercial?.Numero,
                    CEP = funcionario.EnderecoComercial?.CEP,
                    Complemento = funcionario.EnderecoComercial?.Complemento,
                    Bairro = funcionario.EnderecoComercial?.Bairro,
                    Cidade = funcionario.EnderecoComercial?.Cidade,
                    UF = funcionario.EnderecoComercial?.UF,
                }
            };

            _service.Setup(x => x.Atualizar(It.IsAny<Funcionario>(), It.IsAny<string>())).Returns(true);
            var controller = new FuncionarioController(_logger.Object, _service.Object);

            var result = controller.Patch(funcionarioReq) as OkObjectResult;

            _service.Verify(x => x.Atualizar(It.IsAny<Funcionario>(), It.IsAny<string>()), Times.Once);
            result.StatusCode.Should().Be(200);
        }

        [Fact]
        public void Patch_Fail_Test()
        {
            var person = _faker.Person;
            var tels = new List<Telefone> {
                new Telefone("+55","11","90000-0000")
            };
            var endereco = new Endereco("Rua", 10, "00000-000", "apto", "bairro", "cidade", "sp");
            var funcionario = new Funcionario("xxxxxx", "matricular", "cargo",
                new Nome(person.FirstName, person.LastName),
                new DataNascimento(new System.DateTime(1987, 08, 14)),
                new Email(person.Email), tels, endereco, endereco);


            var funcionarioReq = new FuncionarioRequest()
            {
                Ativo = funcionario.Ativo,
                Cargo = funcionario.Cargo,
                DataNascimento = funcionario.DataNascimento.Date,
                Email = funcionario.Email.ToString(),
                Matricula = funcionario.Matricula,
                Nome = funcionario.Nome.PrimeiroNome,
                SobreNome = funcionario.Nome.SobreNome,
                UserId = funcionario.UserId,
                Telefones = funcionario.Telefones.Select(x => new TelefoneRequest
                {
                    DDI = x.DDI,
                    Telefone = $"{x.DDD}{x.NumeroTelefone}"
                }).ToList(),
                EnderecoResidencial = new EnderecoRequest
                {
                    Rua = funcionario.EnderecoResidencial?.Rua,
                    Numero = funcionario.EnderecoResidencial?.Numero,
                    CEP = funcionario.EnderecoResidencial?.CEP,
                    Complemento = funcionario.EnderecoResidencial?.Complemento,
                    Bairro = funcionario.EnderecoResidencial?.Bairro,
                    Cidade = funcionario.EnderecoResidencial?.Cidade,
                    UF = funcionario.EnderecoResidencial?.UF,
                },
                EnderecoComercial = new EnderecoRequest
                {
                    Rua = funcionario.EnderecoComercial?.Rua,
                    Numero = funcionario.EnderecoComercial?.Numero,
                    CEP = funcionario.EnderecoComercial?.CEP,
                    Complemento = funcionario.EnderecoComercial?.Complemento,
                    Bairro = funcionario.EnderecoComercial?.Bairro,
                    Cidade = funcionario.EnderecoComercial?.Cidade,
                    UF = funcionario.EnderecoComercial?.UF,
                }
            };


            _service.Setup(x => x.Atualizar(It.IsAny<Funcionario>(), It.IsAny<string>())).Throws(new Exception());
            var controller = new FuncionarioController(_logger.Object, _service.Object);

            var result = controller.Patch(funcionarioReq) as BadRequestObjectResult;

            _service.Verify(x => x.Atualizar(It.IsAny<Funcionario>(), It.IsAny<string>()), Times.Once);
            result.StatusCode.Should().Be(400);
        }
    }
}
