using Bogus;
using Cadastro.Domain.Interfaces;
using Cadastro.Domain.Services;
using Domain.Entities;
using Domain.ValueObject;
using FluentAssertions;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Moq;
using System.IO;
using System.Threading.Tasks;
using Xunit;

namespace Cadastro.Test.Domain
{
    public class FuncionarioServiceTest
    {
        public readonly Mock<IFuncionarioRepository> _mockFuncionarioRepositorio;
        public readonly Faker _faker;
        public readonly Mock<ILogger<FuncionarioService>> _mockLogger;
        public FuncionarioServiceTest()
        {
            _mockFuncionarioRepositorio = new Mock<IFuncionarioRepository>();
            _faker = new Faker("pt_BR");
            _mockLogger = new Mock<ILogger<FuncionarioService>>();
        }

        public IConfigurationRoot BuildConfiguration()
        {
            return new ConfigurationBuilder()
             .SetBasePath(Directory.GetCurrentDirectory())
                .AddJsonFile("appsettings.json", optional: true)
                .Build();
        }

        [Fact]
        public async Task Cadastrar_OK_Quando_EMail_Nao_Existe()
        {
            var person = _faker.Person;
            var funcionario = new Funcionario("xxxxxx", "matricular", "cargo",
                new Nome(person.FirstName, person.LastName),
                new DataNascimento(new System.DateTime(1987, 08, 14)),
                new Email(person.Email));

            _mockFuncionarioRepositorio.Setup(x => x.Inserir(It.IsAny<Funcionario>()))
                .ReturnsAsync(funcionario.Id);

            _mockFuncionarioRepositorio.Setup(x => x.BuscarPorEmail(It.IsAny<string>()))
                .ReturnsAsync((Funcionario)null)
                .Callback<string>(email =>
                {
                    Assert.Equal(funcionario.Email.EnderecoEmail, email);
                });

            var service = new FuncionarioService(_mockFuncionarioRepositorio.Object, _mockLogger.Object);

            bool result = await service.Cadastrar(funcionario);


            _mockFuncionarioRepositorio.Verify(x => x.BuscarPorEmail(It.IsAny<string>()), Times.Once);
            _mockFuncionarioRepositorio.Verify(x => x.Inserir(It.IsAny<Funcionario>()), Times.Once);
            result.Should().BeTrue();
        }
        [Fact]
        public async Task Cadastrar_Nao_OK_Quando_EMail_Ja_Existe()
        {
            var person = _faker.Person;
            var funcionario = new Funcionario("xxxxxx", "matricular", "cargo",
                new Nome(person.FirstName, person.LastName),
                new DataNascimento(new System.DateTime(1987,08,14)),
                new Email(person.Email));

            _mockFuncionarioRepositorio.Setup(x => x.BuscarPorEmail(It.IsAny<string>()))
                .ReturnsAsync(funcionario)
                .Callback<string>(email =>
                {
                    Assert.Equal(funcionario.Email.EnderecoEmail, email);
                });

            var service = new FuncionarioService(_mockFuncionarioRepositorio.Object, _mockLogger.Object);

            bool result = await service.Cadastrar(funcionario);

            _mockFuncionarioRepositorio.Verify(x => x.BuscarPorEmail(It.IsAny<string>()), Times.Once);
            _mockFuncionarioRepositorio.Verify(x => x.Inserir(It.IsAny<Funcionario>()), Times.Never);
            result.Should().BeFalse();
        }
    }
}
