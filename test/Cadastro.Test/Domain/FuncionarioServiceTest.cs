using Bogus;
using Cadastro.Domain.Interfaces;
using Cadastro.Domain.Services;
using Domain.Entities;
using Domain.ValueObject;
using FluentAssertions;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Moq;
using System.Text.Json;
using System.Text.Json.Serialization;
using System.IO;
using System.Threading.Tasks;
using Xunit;

namespace Cadastro.Test.Domain
{
    public class FuncionarioServiceTest
    {
        public readonly Mock<IFuncionarioReadRepository> _mockFuncionarioRepositorioLeitura;

        public readonly Mock<IFuncionarioWriteRepository> _mockFuncionarioRepositorioEscrita;
        public readonly Faker _faker;
        public readonly Mock<ILogger<FuncionarioService>> _mockLogger;
        public FuncionarioServiceTest()
        {
            _mockFuncionarioRepositorioLeitura = new Mock<IFuncionarioReadRepository>();
            _mockFuncionarioRepositorioEscrita = new Mock<IFuncionarioWriteRepository>();
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

            _mockFuncionarioRepositorioEscrita.Setup(x => x.Inserir(It.IsAny<Funcionario>()))
                .ReturnsAsync(funcionario.Id);

            _mockFuncionarioRepositorioLeitura.Setup(x => x.ObterPorEmail(It.IsAny<string>()))
                .ReturnsAsync((Funcionario)null)
                .Callback<string>(email =>
                {
                    Assert.Equal(funcionario.Email.EnderecoEmail, email);
                });

            var service = new FuncionarioService(_mockFuncionarioRepositorioLeitura.Object, _mockFuncionarioRepositorioEscrita.Object, _mockLogger.Object);

            bool result = await service.Cadastrar(funcionario);

            _mockFuncionarioRepositorioLeitura.Verify(x => x.ObterPorEmail(It.IsAny<string>()), Times.Once);
            _mockFuncionarioRepositorioEscrita.Verify(x => x.Inserir(It.IsAny<Funcionario>()), Times.Once);
            result.Should().BeTrue();
        }

        [Fact]
        public async Task Cadastrar_Nao_OK_Quando_EMail_Ja_Existe()
        {
            var person = _faker.Person;
            var funcionario = new Funcionario("xxxxxx", "matricular", "cargo",
                new Nome(person.FirstName, person.LastName),
                new DataNascimento(new System.DateTime(1987, 08, 14)),
                new Email(person.Email));

            _mockFuncionarioRepositorioLeitura.Setup(x => x.ObterPorEmail(It.IsAny<string>()))
                .ReturnsAsync(funcionario)
                .Callback<string>(email =>
                {
                    Assert.Equal(funcionario.Email.EnderecoEmail, email);
                });

            var service = new FuncionarioService(_mockFuncionarioRepositorioLeitura.Object, _mockFuncionarioRepositorioEscrita.Object, _mockLogger.Object);

            bool result = await service.Cadastrar(funcionario);

            _mockFuncionarioRepositorioLeitura.Verify(x => x.ObterPorEmail(It.IsAny<string>()), Times.Once);
            _mockFuncionarioRepositorioEscrita.Verify(x => x.Inserir(It.IsAny<Funcionario>()), Times.Never);
            result.Should().BeFalse();
        }

        [Fact]
        public void DeserializeJson()
        {
            var json = "{\"rua\":\"teste\",\"numero\":1,\"cep\":\"04679290\",\"complemento\":\"teste\",\"bairro\":\"teste\",\"cidade\":\"teste\",\"uf\":\"sp\"}";
            try
            {

                var temp = JsonSerializer.Deserialize<dynamic>(json);
                Endereco func = JsonSerializer.Deserialize<Endereco>(json);
                func.Should().NotBeNull();
            }
            catch (System.Exception ex)
            {

                throw;
            }
        }
    }
}



