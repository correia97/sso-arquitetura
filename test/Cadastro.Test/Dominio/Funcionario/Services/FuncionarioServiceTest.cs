using Cadastro.Domain.Interfaces;
using Cadastro.Domain.Services;
using Domain.Entities;
using Domain.ValueObject;
using Microsoft.Extensions.Logging;
using Xunit;
using Xunit.Abstractions;

namespace Cadastro.Test.Domain
{
    public class FuncionarioServiceTest
    {
        public readonly Mock<IFuncionarioReadRepository> _mockFuncionarioRepositorioLeitura;

        public readonly Mock<IFuncionarioWriteRepository> _mockFuncionarioRepositorioEscrita;
        public readonly Faker _faker;
        public readonly Mock<ILogger<FuncionarioService>> _mockLogger;
        private ITestOutputHelper Output { get; }

        public FuncionarioServiceTest(ITestOutputHelper outputHelper)
        {
            _mockFuncionarioRepositorioLeitura = new Mock<IFuncionarioReadRepository>();
            _mockFuncionarioRepositorioEscrita = new Mock<IFuncionarioWriteRepository>();
            _faker = new Faker("pt_BR");
            _mockLogger = new Mock<ILogger<FuncionarioService>>();
            Output = outputHelper;
        }

        #region Cadastrar

        [Fact]
        public async Task Cadastrar_OK_Quando_EMail_Nao_Existe()
        {
            var person = _faker.Person;
            var funcionario = new Funcionario(Guid.NewGuid().ToString(), "matricular", "cargo",
                new Nome(person.FirstName, person.LastName),
                new DataNascimento(new System.DateTime(1987, 08, 14)),
                new Email(person.Email));

            _mockFuncionarioRepositorioEscrita.Setup(x => x.Inserir(It.IsAny<Funcionario>()))
                .ReturnsAsync(funcionario.Id);

            _mockFuncionarioRepositorioLeitura.Setup(x => x.ObterPorEmail(It.IsAny<string>()))
                .ReturnsAsync((Funcionario)null)
                .Callback<string>(email =>
                {
                    Output.WriteLine($"Callback Email: {email}");
                    Assert.Equal(funcionario.Email.EnderecoEmail, email);
                });

            var service = new FuncionarioService(_mockFuncionarioRepositorioLeitura.Object, _mockFuncionarioRepositorioEscrita.Object, _mockLogger.Object);

            bool result = await service.Cadastrar(funcionario);

            Output.WriteLine($"Result: {result}");

            _mockFuncionarioRepositorioLeitura.Verify(x => x.ObterPorEmail(It.IsAny<string>()), Times.Once);
            _mockFuncionarioRepositorioEscrita.Verify(x => x.Inserir(It.IsAny<Funcionario>()), Times.Once);
            result.Should().BeTrue();
        }

        [Fact]
        public async Task Cadastrar_Nao_OK_Quando_EMail_Ja_Existe()
        {
            var person = _faker.Person;
            var funcionario = new Funcionario(Guid.NewGuid().ToString(), "matricular", "cargo",
                new Nome(person.FirstName, person.LastName),
                new DataNascimento(new System.DateTime(1987, 08, 14)),
                new Email(person.Email));

            _mockFuncionarioRepositorioLeitura.Setup(x => x.ObterPorEmail(It.IsAny<string>()))
                .ReturnsAsync(funcionario)
                .Callback<string>(email =>
                {
                    Output.WriteLine($"Callback Email: {email}");
                    Assert.Equal(funcionario.Email.EnderecoEmail, email);
                });

            var service = new FuncionarioService(_mockFuncionarioRepositorioLeitura.Object, _mockFuncionarioRepositorioEscrita.Object, _mockLogger.Object);

            bool result = await service.Cadastrar(funcionario);

            Output.WriteLine($"Result: {result}");

            _mockFuncionarioRepositorioLeitura.Verify(x => x.ObterPorEmail(It.IsAny<string>()), Times.Once);
            _mockFuncionarioRepositorioEscrita.Verify(x => x.Inserir(It.IsAny<Funcionario>()), Times.Never);
            result.Should().BeFalse();
        }

        [Fact]
        public async Task Cadastrar_Nao_OK_Quando_Banco_Nao_Acessivel()
        {
            var person = _faker.Person;
            var funcionario = new Funcionario(Guid.NewGuid().ToString(), "matricular", "cargo",
                new Nome(person.FirstName, person.LastName),
                new DataNascimento(new System.DateTime(1987, 08, 14)),
                new Email(person.Email));

            _mockFuncionarioRepositorioLeitura.Setup(x => x.ObterPorEmail(It.IsAny<string>()))
                .Throws(new Exception());

            var service = new FuncionarioService(_mockFuncionarioRepositorioLeitura.Object, _mockFuncionarioRepositorioEscrita.Object, _mockLogger.Object);

            bool result = await service.Cadastrar(funcionario);

            Output.WriteLine($"Result: {result}");

            _mockFuncionarioRepositorioLeitura.Verify(x => x.ObterPorEmail(It.IsAny<string>()), Times.Once);
            result.Should().BeFalse();
        }

        [Fact]
        public async Task CadastrarComEnderecoTelefone_OK_Quando_EMail_Nao_Existe()
        {
            var person = _faker.Person;
            var id = Guid.NewGuid();
            var tels = new List<Telefone> {
                new Telefone("+55","11","90000-0000", id)
            };
            var endereco = new Endereco("Rua", 10, "00000-000", "apto", "bairro", "cidade", "sp", Cadastro.Domain.Enums.TipoEnderecoEnum.Residencial, id);
            var funcionario = new Funcionario(id.ToString(), "matricular", "cargo",
                new Nome(person.FirstName, person.LastName),
                new DataNascimento(new System.DateTime(1987, 08, 14)),
                new Email(person.Email), tels, endereco, endereco);

            _mockFuncionarioRepositorioEscrita.Setup(x => x.Inserir(It.IsAny<Funcionario>()))
                .ReturnsAsync(funcionario.Id);

            _mockFuncionarioRepositorioLeitura.Setup(x => x.ObterPorEmail(It.IsAny<string>()))
                .ReturnsAsync((Funcionario)null)
                .Callback<string>(email =>
                {
                    Output.WriteLine($"Callback Email: {email}");
                    Assert.Equal(funcionario.Email.EnderecoEmail, email);
                });

            var service = new FuncionarioService(_mockFuncionarioRepositorioLeitura.Object, _mockFuncionarioRepositorioEscrita.Object, _mockLogger.Object);

            bool result = await service.Cadastrar(funcionario);

            Output.WriteLine($"Result: {result}");

            _mockFuncionarioRepositorioLeitura.Verify(x => x.ObterPorEmail(It.IsAny<string>()), Times.Once);
            _mockFuncionarioRepositorioEscrita.Verify(x => x.Inserir(It.IsAny<Funcionario>()), Times.Once);
            result.Should().BeTrue();
        }

        [Fact]
        public async Task CadastrarComEnderecoTelefone_Nao_OK_Quando_EMail_Ja_Existe()
        {
            var person = _faker.Person;
            var id = Guid.NewGuid();
            var tels = new List<Telefone> {
                new Telefone("+55","11","90000-0000",id)
            };
            var endereco = new Endereco("Rua", 10, "00000-000", "apto", "bairro", "cidade", "sp", Cadastro.Domain.Enums.TipoEnderecoEnum.Comercial, id);
            var funcionario = new Funcionario(id.ToString(), "matricular", "cargo",
                new Nome(person.FirstName, person.LastName),
                new DataNascimento(new System.DateTime(1987, 08, 14)),
                new Email(person.Email), tels, endereco, endereco);

            _mockFuncionarioRepositorioLeitura.Setup(x => x.ObterPorEmail(It.IsAny<string>()))
                .ReturnsAsync(funcionario)
                .Callback<string>(email =>
                {
                    Output.WriteLine($"Callback Email: {email}");
                    Assert.Equal(funcionario.Email.EnderecoEmail, email);
                });

            var service = new FuncionarioService(_mockFuncionarioRepositorioLeitura.Object, _mockFuncionarioRepositorioEscrita.Object, _mockLogger.Object);

            bool result = await service.Cadastrar(funcionario);

            Output.WriteLine($"Result: {result}");

            _mockFuncionarioRepositorioLeitura.Verify(x => x.ObterPorEmail(It.IsAny<string>()), Times.Once);
            _mockFuncionarioRepositorioEscrita.Verify(x => x.Inserir(It.IsAny<Funcionario>()), Times.Never);
            result.Should().BeFalse();
        }
        #endregion

        #region Atualizar

        [Fact]
        public async Task AtualizarComEnderecoTelefone_Nao_OK_Quando_Registro_Nao_Existe()
        {
            var person = _faker.Person;
            var id = Guid.NewGuid();
            var tels = new List<Telefone> {
                new Telefone("+55","11","90000-0000",id)
            };
            var endereco = new Endereco("Rua", 10, "00000-000", "apto", "bairro", "cidade", "sp", Cadastro.Domain.Enums.TipoEnderecoEnum.Comercial, id);
            var funcionario = new Funcionario(id.ToString(), "matricular", "cargo",
                new Nome(person.FirstName, person.LastName),
                new DataNascimento(new System.DateTime(1987, 08, 14)),
                new Email(person.Email), tels, endereco, endereco);

            var telsNovo = new List<Telefone> {
                new Telefone("+55","11","90000-0000",id),
                new Telefone("+55","11","80000-0000", id)
            };
            var enderecoNovo = new Endereco("Rua", 11, "00000-000", "apt", "bairro", "cidade", "sp", Cadastro.Domain.Enums.TipoEnderecoEnum.Residencial, id);
            var funcionarioAtualizado = new Funcionario(id.ToString(), "matricular", "cargo 2",
                new Nome(person.FirstName, "Silva Sauro"),
                new DataNascimento(new System.DateTime(1985, 08, 14)),
                new Email(person.Email), telsNovo, enderecoNovo, enderecoNovo);

            _mockFuncionarioRepositorioEscrita.Setup(x => x.Atualizar(It.IsAny<Funcionario>()))
                .ReturnsAsync(true)
                .Callback<Funcionario>(atualizado =>
                {
                    Output.WriteLine($"Callback Atualizar: {atualizado.ToJson()}");
                    atualizado.Nome.SobreNome.Should().Be(funcionarioAtualizado.Nome.SobreNome);
                    atualizado.EnderecoComercial.Numero.Should().Be(funcionarioAtualizado.EnderecoComercial.Numero);
                    atualizado.EnderecoComercial.Complemento.Should().Be(funcionarioAtualizado.EnderecoComercial.Complemento);
                    atualizado.DataNascimento.Date.Should().Be(funcionarioAtualizado.DataNascimento.Date);
                    atualizado.Nome.SobreNome.Should().Be(funcionarioAtualizado.Nome.SobreNome);
                    atualizado.Cargo.Should().Be(funcionarioAtualizado.Cargo);
                    atualizado.Telefones.Should().HaveCount(2);

                });

            _mockFuncionarioRepositorioLeitura.Setup(x => x.ObterPorId(It.IsAny<Guid>()))
                .ReturnsAsync(funcionario)
                .Callback<Guid>(id =>
                {
                    Output.WriteLine($"Callback Email: {id}");
                    Assert.Equal(funcionarioAtualizado.Id, id);
                });

            var service = new FuncionarioService(_mockFuncionarioRepositorioLeitura.Object, _mockFuncionarioRepositorioEscrita.Object, _mockLogger.Object);

            bool result = await service.Atualizar(funcionarioAtualizado, "");

            Output.WriteLine($"Result: {result}");

            _mockFuncionarioRepositorioLeitura.Verify(x => x.ObterPorId(It.IsAny<Guid>()), Times.Once);
            _mockFuncionarioRepositorioEscrita.Verify(x => x.Atualizar(It.IsAny<Funcionario>()), Times.Once);
            result.Should().BeTrue();
        }

        [Fact]
        public async Task AtualizarComEnderecoTelefone_OK_Quando_Registro_Existe()
        {
            var person = _faker.Person;
            var id = Guid.NewGuid();

            var telsNovo = new List<Telefone> {
                new Telefone("+55","11","90000-0000", id),
                new Telefone("+55","11","80000-0000", id)
            };
            var enderecoNovo = new Endereco("Rua", 11, "00000-000", "apt", "bairro", "cidade", "sp", Cadastro.Domain.Enums.TipoEnderecoEnum.Comercial, id);
            var funcionarioAtualizado = new Funcionario(id.ToString(), "matricular", "cargo 2",
                new Nome(person.FirstName, "Silva Sauro"),
                new DataNascimento(new System.DateTime(1985, 08, 14)),
                new Email(person.Email), telsNovo, enderecoNovo, enderecoNovo);


            _mockFuncionarioRepositorioLeitura.Setup(x => x.ObterPorId(It.IsAny<Guid>()))
                .ReturnsAsync((Funcionario)null)
                .Callback<Guid>(id =>
                {
                    Output.WriteLine($"Callback Email: {id}");
                });

            var service = new FuncionarioService(_mockFuncionarioRepositorioLeitura.Object, _mockFuncionarioRepositorioEscrita.Object, _mockLogger.Object);

            bool result = await service.Atualizar(funcionarioAtualizado, "");

            Output.WriteLine($"Result: {result}");

            _mockFuncionarioRepositorioLeitura.Verify(x => x.ObterPorId(It.IsAny<Guid>()), Times.Once);
            result.Should().BeFalse();
        }
        #endregion

        #region ObterPorId

        [Fact]
        public async Task ObterPorId_OK_Quando_Registro_Existe()
        {
            var person = _faker.Person;
            var id = Guid.NewGuid();
            var tels = new List<Telefone> {
                new Telefone("+55","11","90000-0000", id)
            };
            var endereco = new Endereco("Rua", 10, "00000-000", "apto", "bairro", "cidade", "sp", Cadastro.Domain.Enums.TipoEnderecoEnum.Residencial, id);
            var funcionario = new Funcionario(id.ToString(), "matricular", "cargo",
                new Nome(person.FirstName, person.LastName),
                new DataNascimento(new System.DateTime(1987, 08, 14)),
                new Email(person.Email), tels, endereco, endereco);

            _mockFuncionarioRepositorioLeitura.Setup(x => x.ObterPorId(It.IsAny<Guid>()))
                .ReturnsAsync(funcionario)
                .Callback<Guid>(id =>
                {
                    Output.WriteLine($"Callback Email: {id}");
                    Assert.Equal(funcionario.Id, id);
                });

            var service = new FuncionarioService(_mockFuncionarioRepositorioLeitura.Object, _mockFuncionarioRepositorioEscrita.Object, _mockLogger.Object);

            var result = await service.ObterPorId(funcionario.Id);

            Output.WriteLine($"Result: {result}");

            _mockFuncionarioRepositorioLeitura.Verify(x => x.ObterPorId(It.IsAny<Guid>()), Times.Once);
            result.Should().NotBeNull();
        }

        [Fact]
        public async Task ObterPorId_OK_Quando_Registro_Nao_Existe()
        {

            _mockFuncionarioRepositorioLeitura.Setup(x => x.ObterPorId(It.IsAny<Guid>()))
                .ReturnsAsync((Funcionario)null)
                .Callback<Guid>(id =>
                {
                    Output.WriteLine($"Callback Email: {id}");
                });

            var service = new FuncionarioService(_mockFuncionarioRepositorioLeitura.Object, _mockFuncionarioRepositorioEscrita.Object, _mockLogger.Object);

            var result = await service.ObterPorId(Guid.NewGuid());

            Output.WriteLine($"Result: {result}");

            _mockFuncionarioRepositorioLeitura.Verify(x => x.ObterPorId(It.IsAny<Guid>()), Times.Once);
            result.Should().BeNull();
        }

        [Fact]
        public async Task ObterPorId_OK_Quando_Base_Nao_Disponivel()
        {
            _mockFuncionarioRepositorioLeitura.Setup(x => x.ObterPorId(It.IsAny<Guid>()))
                .Throws(new Exception("Teste"));

            var service = new FuncionarioService(_mockFuncionarioRepositorioLeitura.Object, _mockFuncionarioRepositorioEscrita.Object, _mockLogger.Object);

            var result = await service.ObterPorId(Guid.NewGuid());

            Output.WriteLine($"Result: {result}");

            _mockFuncionarioRepositorioLeitura.Verify(x => x.ObterPorId(It.IsAny<Guid>()), Times.Once);
            result.Should().BeNull();
        }
        #endregion

        #region ObterPorTodos

        [Fact]
        public async Task ObterPorTodos_OK_Quando_Registro_Existe()
        {
            var person = _faker.Person;
            var id = Guid.NewGuid();
            var tels = new List<Telefone> {
                new Telefone("+55","11","90000-0000", id)
            };
            var endereco = new Endereco("Rua", 10, "00000-000", "apto", "bairro", "cidade", "sp", Cadastro.Domain.Enums.TipoEnderecoEnum.Residencial, id);
            var funcionario = new Funcionario(id.ToString(), "matricular", "cargo",
                new Nome(person.FirstName, person.LastName),
                new DataNascimento(new System.DateTime(1987, 08, 14)),
                new Email(person.Email), tels, endereco, endereco);

            _mockFuncionarioRepositorioLeitura.Setup(x => x.ObterTodos())
                .ReturnsAsync(new List<Funcionario>() { funcionario });

            var service = new FuncionarioService(_mockFuncionarioRepositorioLeitura.Object, _mockFuncionarioRepositorioEscrita.Object, _mockLogger.Object);

            var result = await service.ObterTodos();

            Output.WriteLine($"Result: {result}");

            _mockFuncionarioRepositorioLeitura.Verify(x => x.ObterTodos(), Times.Once);
            result.Should().NotBeNull();
            result.Should().HaveCount(1);
        }

        [Fact]
        public async Task ObterPorTodos_OK_Quando_Registros_Nao_Existem()
        {

            _mockFuncionarioRepositorioLeitura.Setup(x => x.ObterTodos())
                .ReturnsAsync(new List<Funcionario>());

            var service = new FuncionarioService(_mockFuncionarioRepositorioLeitura.Object, _mockFuncionarioRepositorioEscrita.Object, _mockLogger.Object);

            var result = await service.ObterTodos();

            Output.WriteLine($"Result: {result}");

            _mockFuncionarioRepositorioLeitura.Verify(x => x.ObterTodos(), Times.Once);
            result.Should().NotBeNull();
            result.Should().HaveCount(0);
        }

        [Fact]
        public async Task ObterPorTodos_OK_Quando_Base_Nao_Disponivel()
        {

            _mockFuncionarioRepositorioLeitura.Setup(x => x.ObterTodos())
                .Throws(new Exception("Teste"));

            var service = new FuncionarioService(_mockFuncionarioRepositorioLeitura.Object, _mockFuncionarioRepositorioEscrita.Object, _mockLogger.Object);

            var result = await service.ObterTodos();

            Output.WriteLine($"Result: {result}");

            _mockFuncionarioRepositorioLeitura.Verify(x => x.ObterTodos(), Times.Once);
            result.Should().BeNull();
        }
        #endregion
    }
}