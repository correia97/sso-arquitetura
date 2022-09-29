using Cadastro.API.Services;
using Cadastro.Domain.Services;
using Domain.Entities;
using Domain.ValueObject;
using Microsoft.Extensions.Logging;
using RabbitMQ.Client;
using Xunit;
using Xunit.Abstractions;

namespace Cadastro.Test.Apresentacao.API.Services
{
    public class FuncionarioAppServiceTest
    {
        private Mock< IFuncionarioService> _mockService;
        private Mock<ILogger<FuncionarioAppService>> _mockLogger;
        private Mock<IModel> _mockModel;
        private Mock<IBasicProperties> _mockBasicProperties;

        public readonly Faker _faker;
        private ITestOutputHelper Output { get; }

        public FuncionarioAppServiceTest(ITestOutputHelper outputHelper)
        {
            _mockService = new Mock<IFuncionarioService>();
            _mockLogger = new Mock<ILogger<FuncionarioAppService>>();
            _mockModel = new Mock<IModel>();
            _mockBasicProperties = new Mock<IBasicProperties>();
            _faker = new Faker("pt_BR");
            Output = outputHelper;
        }

        [Fact]
        public void Cadastrar_Teste()
        {
            _mockModel.Setup(x => x.CreateBasicProperties())
                .Returns(_mockBasicProperties.Object);
            var appService = new FuncionarioAppService(_mockModel.Object,_mockService.Object, _mockLogger.Object);
            var person = _faker.Person;
            var funcionario = new Funcionario(Guid.NewGuid().ToString(), "matricular", "cargo",
                new Nome(person.FirstName, person.LastName),
                new DataNascimento(new System.DateTime(1987, 08, 14)),
                new Email(person.Email));

            appService.Cadastrar(funcionario, Guid.NewGuid());

            _mockModel.Verify(x => x.CreateBasicProperties(), Times.Once);
        }
        [Fact]
        public void Atualizar_Teste()
        {
            _mockModel.Setup(x => x.CreateBasicProperties())
                .Returns(_mockBasicProperties.Object);
            var appService = new FuncionarioAppService(_mockModel.Object, _mockService.Object, _mockLogger.Object);
            var person = _faker.Person;
            var funcionario = new Funcionario(Guid.NewGuid().ToString(), "matricular", "cargo",
                new Nome(person.FirstName, person.LastName),
                new DataNascimento(new System.DateTime(1987, 08, 14)),
                new Email(person.Email));

            appService.Atualizar(funcionario, Guid.NewGuid());

            _mockModel.Verify(x => x.CreateBasicProperties(), Times.Once);
        }
        [Fact]
        public void Desativar_Teste()
        {
            _mockModel.Setup(x => x.CreateBasicProperties())
                .Returns(_mockBasicProperties.Object);
            var appService = new FuncionarioAppService(_mockModel.Object, _mockService.Object, _mockLogger.Object);           

            appService.Desativar(Guid.NewGuid(), Guid.NewGuid());

            _mockModel.Verify(x => x.CreateBasicProperties(), Times.Once);
        }
        [Fact]
        public void Remover_Teste()
        {
            _mockModel.Setup(x => x.CreateBasicProperties())
                .Returns(_mockBasicProperties.Object);
            var appService = new FuncionarioAppService(_mockModel.Object, _mockService.Object, _mockLogger.Object);
            appService.Remover(Guid.NewGuid(), Guid.NewGuid());
            _mockModel.Verify(x => x.CreateBasicProperties(), Times.Once);
        }

        [Fact]
        public async void ObterPorId_Teste()
        {
            var person = _faker.Person;
            var id = Guid.NewGuid();
            var funcionario = new Funcionario(id.ToString(), "matricular", "cargo",
                new Nome(person.FirstName, person.LastName),
                new DataNascimento(new System.DateTime(1987, 08, 14)),
                new Email(person.Email));
            _mockService.Setup(x => x.ObterPorId(It.IsAny<Guid>()))
                .ReturnsAsync(funcionario)
                .Callback<Guid>(currentid =>
                {
                    Output.WriteLine($"id: {currentid}");
                    currentid.Should().Be(id);
                });

            var appService = new FuncionarioAppService(_mockModel.Object, _mockService.Object, _mockLogger.Object);

            var result = await appService.ObterPorId(id);

            _mockService.Verify(x => x.ObterPorId(It.IsAny<Guid>()), Times.Once);
            result.Nome.Should().Be(funcionario.Nome.PrimeiroNome);
            result.SobreNome.Should().Be(funcionario.Nome.SobreNome);
            result.Email.Should().Be(funcionario.Email.EnderecoEmail);
            result.DataNascimento.Should().Be(funcionario.DataNascimento.Date);
           
        }

        [Fact]
        public async void ObterPorId_Nulo_Qdo_Id_Nao_Existe()
        {
            var id = Guid.NewGuid();
            _mockService.Setup(x => x.ObterPorId(It.IsAny<Guid>()))
                .ReturnsAsync((Funcionario)null)
                .Callback<Guid>(currentid =>
                {
                    Output.WriteLine($"id: {currentid}");
                    currentid.Should().Be(id);
                });

            var appService = new FuncionarioAppService(_mockModel.Object, _mockService.Object, _mockLogger.Object);

            var result = await appService.ObterPorId(id);

            _mockService.Verify(x => x.ObterPorId(It.IsAny<Guid>()), Times.Once);
            result.Should().BeNull();
        }

        [Fact]
        public async void ObterTodosTeste()
        {
            var person = _faker.Person;
            var id = Guid.NewGuid();
            var funcionario = new Funcionario(id.ToString(), "matricular", "cargo",
                new Nome(person.FirstName, person.LastName),
                new DataNascimento(new System.DateTime(1987, 08, 14)),
                new Email(person.Email));
            _mockService.Setup(x => x.ObterTodos())
                .ReturnsAsync(new List<Funcionario> { funcionario });

            var appService = new FuncionarioAppService(_mockModel.Object, _mockService.Object, _mockLogger.Object);
            var result = await appService.ObterTodos();
            _mockService.Verify(x => x.ObterTodos(), Times.Once);
            result.Should().HaveCount(1);
        }
    }
}
