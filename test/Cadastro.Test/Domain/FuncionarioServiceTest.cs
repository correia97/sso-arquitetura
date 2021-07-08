using System;
using System.Collections.Generic;
using System.Text;
using Xunit;
using FluentAssertions;
using Bogus;
using Moq;
using Cadastro.Domain.Interfaces;
using Cadastro.Domain.Services;
using Microsoft.Extensions.Logging;
using System.Threading.Tasks;
using Domain.Entities;
using Domain.ValueObject;

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

        [Fact]
        public async Task Cadastrar_OK_Quando_EMail_Nao_Existe()
        {
            var person = _faker.Person;
            var funcionario = new Funcionario("xxxxxx", "matricular", "cargo",
                new Nome(person.FirstName, person.LastName),
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
