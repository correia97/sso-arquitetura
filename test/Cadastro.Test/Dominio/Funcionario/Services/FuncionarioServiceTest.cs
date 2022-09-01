using Cadastro.Domain.Enums;
using Cadastro.Domain.Interfaces;
using Cadastro.Domain.Services;
using Domain.Entities;
using Domain.ValueObject;
using Microsoft.Extensions.Logging;
using System.Data;
using Xunit;
using Xunit.Abstractions;

namespace Cadastro.Test.Domain
{
    public class FuncionarioServiceTest
    {
        public readonly Mock<IDbConnection> _mockConexao;
        public readonly Mock<IDbTransaction> _mockTransacao;
        public readonly Mock<IFuncionarioReadRepository> _mockFuncionarioRepositorioLeitura;
        public readonly Mock<IFuncionarioWriteRepository> _mockFuncionarioRepositorioEscrita;
        public readonly Faker _faker;
        public readonly Mock<ILogger<FuncionarioService>> _mockLogger;
        private ITestOutputHelper Output { get; }

        public FuncionarioServiceTest(ITestOutputHelper outputHelper)
        {
            _mockFuncionarioRepositorioLeitura = new Mock<IFuncionarioReadRepository>();
            _mockFuncionarioRepositorioEscrita = new Mock<IFuncionarioWriteRepository>();
            _mockTransacao = new Mock<IDbTransaction>();
            _mockConexao = new Mock<IDbConnection>();
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

            _mockFuncionarioRepositorioEscrita.Setup(x => x.IniciarTransacao())
                .Returns(_mockTransacao.Object);

            _mockFuncionarioRepositorioEscrita.Setup(x => x.Inserir(It.IsAny<Funcionario>(), It.IsAny<IDbTransaction>()))
                .ReturnsAsync(funcionario.Id);

            _mockFuncionarioRepositorioLeitura.Setup(x => x.ObterPorEmail(It.IsAny<IDbTransaction>(), It.IsAny<string>()))
                .ReturnsAsync((Funcionario)null)
                .Callback<IDbTransaction, string>((transacao, email) =>
                {
                    Output.WriteLine($"Callback Email: {email}");
                    Assert.Equal(funcionario.Email.EnderecoEmail, email);
                });

            var service = new FuncionarioService(_mockFuncionarioRepositorioLeitura.Object, _mockFuncionarioRepositorioEscrita.Object, _mockLogger.Object);

            await service.Cadastrar(funcionario);

            Output.WriteLine($"Result: ok");

            _mockFuncionarioRepositorioLeitura.Verify(x => x.ObterPorEmail(It.IsAny<IDbTransaction>(), It.IsAny<string>()), Times.Once);
            _mockFuncionarioRepositorioEscrita.Verify(x => x.Inserir(It.IsAny<Funcionario>(), It.IsAny<IDbTransaction>()), Times.Once);
        }

        [Fact]
        public async Task Cadastrar_Nao_OK_Quando_EMail_Ja_Existe()
        {
            var person = _faker.Person;
            var funcionario = new Funcionario(Guid.NewGuid().ToString(), "matricular", "cargo",
                new Nome(person.FirstName, person.LastName),
                new DataNascimento(new System.DateTime(1987, 08, 14)),
                new Email(person.Email));

            _mockFuncionarioRepositorioEscrita.Setup(x => x.IniciarTransacao())
                .Returns(_mockTransacao.Object);

            _mockFuncionarioRepositorioLeitura.Setup(x => x.ObterPorEmail(It.IsAny<IDbTransaction>(), It.IsAny<string>()))
                .ReturnsAsync(funcionario)
                .Callback<IDbTransaction, string>((transacao, email) =>
                {
                    Output.WriteLine($"Callback Email: {email}");
                    Assert.Equal(funcionario.Email.EnderecoEmail, email);
                });

            var service = new FuncionarioService(_mockFuncionarioRepositorioLeitura.Object, _mockFuncionarioRepositorioEscrita.Object, _mockLogger.Object);

            await Assert.ThrowsAsync<InvalidOperationException>(async () => await service.Cadastrar(funcionario));

            Output.WriteLine($"Result: ok");

            _mockFuncionarioRepositorioLeitura.Verify(x => x.ObterPorEmail(It.IsAny<IDbTransaction>(), It.IsAny<string>()), Times.Once);
            _mockFuncionarioRepositorioEscrita.Verify(x => x.Inserir(It.IsAny<Funcionario>(), It.IsAny<IDbTransaction>()), Times.Never);
        }

        [Fact]
        public async Task Cadastrar_Nao_OK_Quando_Banco_Nao_Acessivel()
        {
            var person = _faker.Person;
            var funcionario = new Funcionario(Guid.NewGuid().ToString(), "matricular", "cargo",
                new Nome(person.FirstName, person.LastName),
                new DataNascimento(new System.DateTime(1987, 08, 14)),
                new Email(person.Email));

            _mockFuncionarioRepositorioEscrita.Setup(x => x.IniciarTransacao())
                .Returns(_mockTransacao.Object);

            _mockFuncionarioRepositorioLeitura.Setup(x => x.ObterPorEmail(It.IsAny<IDbTransaction>(), It.IsAny<string>()))
                .Throws(new Exception());

            var service = new FuncionarioService(_mockFuncionarioRepositorioLeitura.Object, _mockFuncionarioRepositorioEscrita.Object, _mockLogger.Object);

            await Assert.ThrowsAsync<Exception>(async () => await service.Cadastrar(funcionario));

            Output.WriteLine($"Result: ok");

            _mockFuncionarioRepositorioLeitura.Verify(x => x.ObterPorEmail(It.IsAny<IDbTransaction>(), It.IsAny<string>()), Times.Once);
            _mockFuncionarioRepositorioEscrita.Verify(x => x.IniciarTransacao(), Times.Once);
        }

        [Fact]
        public async Task CadastrarComEnderecoTelefone_OK_Quando_EMail_Nao_Existe()
        {
            var person = _faker.Person;
            var id = Guid.NewGuid();
            var tels = new List<Telefone> {
                new Telefone("+55","11","90000-0000", id)
            };
            var endereco = new Endereco("Rua", 10, "00000-000", "apto", "bairro", "cidade", "sp", TipoEnderecoEnum.Residencial, id);
            var funcionario = new Funcionario(id.ToString(), "matricular", "cargo",
                new Nome(person.FirstName, person.LastName),
                new DataNascimento(new System.DateTime(1987, 08, 14)),
                new Email(person.Email), tels, endereco, endereco);

            _mockFuncionarioRepositorioEscrita.Setup(x => x.IniciarTransacao())
                .Returns(_mockTransacao.Object);

            _mockFuncionarioRepositorioEscrita.Setup(x => x.Inserir(It.IsAny<Funcionario>(), It.IsAny<IDbTransaction>()))
                .ReturnsAsync(funcionario.Id);

            _mockFuncionarioRepositorioLeitura.Setup(x => x.ObterPorEmail(It.IsAny<IDbTransaction>(), It.IsAny<string>()))
                .ReturnsAsync((Funcionario)null)
                .Callback<IDbTransaction, string>((transaction, email) =>
                {
                    Output.WriteLine($"Callback Email: {email}");
                    Assert.Equal(funcionario.Email.EnderecoEmail, email);
                });

            var service = new FuncionarioService(_mockFuncionarioRepositorioLeitura.Object, _mockFuncionarioRepositorioEscrita.Object, _mockLogger.Object);

            await service.Cadastrar(funcionario);

            Output.WriteLine($"Result: ok");

            _mockFuncionarioRepositorioLeitura.Verify(x => x.ObterPorEmail(It.IsAny<IDbTransaction>(), It.IsAny<string>()), Times.Once);
            _mockFuncionarioRepositorioEscrita.Verify(x => x.Inserir(It.IsAny<Funcionario>(), It.IsAny<IDbTransaction>()), Times.Once);
            _mockFuncionarioRepositorioEscrita.Verify(x => x.IniciarTransacao(), Times.Once);
        }

        [Fact]
        public async Task CadastrarComEnderecoTelefone_Nao_OK_Quando_EMail_Ja_Existe()
        {
            var person = _faker.Person;
            var id = Guid.NewGuid();
            var tels = new List<Telefone> {
                new Telefone("+55","11","90000-0000",id)
            };
            var endereco = new Endereco("Rua", 10, "00000-000", "apto", "bairro", "cidade", "sp", TipoEnderecoEnum.Comercial, id);
            var funcionario = new Funcionario(id.ToString(), "matricular", "cargo",
                new Nome(person.FirstName, person.LastName),
                new DataNascimento(new DateTime(1987, 08, 14)),
                new Email(person.Email), tels, endereco, endereco);

            _mockFuncionarioRepositorioEscrita.Setup(x => x.IniciarTransacao())
                .Returns(_mockTransacao.Object);

            _mockFuncionarioRepositorioLeitura.Setup(x => x.ObterPorEmail(It.IsAny<IDbTransaction>(), It.IsAny<string>()))
                .ReturnsAsync(funcionario)
                .Callback<IDbTransaction, string>((transaction, email) =>
                {
                    Output.WriteLine($"Callback Email: {email}");
                    Assert.Equal(funcionario.Email.EnderecoEmail, email);
                });

            var service = new FuncionarioService(_mockFuncionarioRepositorioLeitura.Object, _mockFuncionarioRepositorioEscrita.Object, _mockLogger.Object);

            await Assert.ThrowsAsync<InvalidOperationException>(async () => await service.Cadastrar(funcionario));

            Output.WriteLine($"Result: ok");

            _mockFuncionarioRepositorioLeitura.Verify(x => x.ObterPorEmail(It.IsAny<IDbTransaction>(), It.IsAny<string>()), Times.Once);
            _mockFuncionarioRepositorioEscrita.Verify(x => x.IniciarTransacao(), Times.Once);
            _mockFuncionarioRepositorioEscrita.Verify(x => x.Inserir(It.IsAny<Funcionario>(), It.IsAny<IDbTransaction>()), Times.Never);
        }


        [Fact]
        public async Task CadastrarComEnderecoTelefone_Nao_OK_Quando_Erro_Inserir()
        {
            var person = _faker.Person;
            var id = Guid.NewGuid();
            var tels = new List<Telefone> {
                new Telefone("+55","11","90000-0000",id)
            };
            var endereco = new Endereco("Rua", 10, "00000-000", "apto", "bairro", "cidade", "sp", TipoEnderecoEnum.Comercial, id);
            var funcionario = new Funcionario(id.ToString(), "matricular", "cargo",
                new Nome(person.FirstName, person.LastName),
                new DataNascimento(new DateTime(1987, 08, 14)),
                new Email(person.Email), tels, endereco, endereco);

            _mockFuncionarioRepositorioEscrita.Setup(x => x.IniciarTransacao())
                .Returns(_mockTransacao.Object);

            _mockFuncionarioRepositorioEscrita.Setup(x => x.CancelarTransacao(It.IsAny<IDbTransaction>()));

            _mockFuncionarioRepositorioLeitura.Setup(x => x.ObterPorEmail(It.IsAny<IDbTransaction>(), It.IsAny<string>()))
                .ReturnsAsync((Funcionario)null)
                .Callback<IDbTransaction, string>((transaction, email) =>
                {
                    Output.WriteLine($"Callback Email: {email}");
                    Assert.Equal(funcionario.Email.EnderecoEmail, email);
                });

            _mockFuncionarioRepositorioEscrita.Setup(x => x.Inserir(It.IsAny<Funcionario>(), It.IsAny<IDbTransaction>())).ReturnsAsync(Guid.Empty);

            var service = new FuncionarioService(_mockFuncionarioRepositorioLeitura.Object, _mockFuncionarioRepositorioEscrita.Object, _mockLogger.Object);

            await Assert.ThrowsAsync<InvalidOperationException>(async () => await service.Cadastrar(funcionario));

            Output.WriteLine($"Result: ok");

            _mockFuncionarioRepositorioLeitura.Verify(x => x.ObterPorEmail(It.IsAny<IDbTransaction>(), It.IsAny<string>()), Times.Once);
            _mockFuncionarioRepositorioEscrita.Verify(x => x.Inserir(It.IsAny<Funcionario>(), It.IsAny<IDbTransaction>()), Times.Once);
            _mockFuncionarioRepositorioEscrita.Verify(x => x.IniciarTransacao(), Times.Once);
            _mockFuncionarioRepositorioEscrita.Verify(x => x.CancelarTransacao(It.IsAny<IDbTransaction>()), Times.Once);
        }

        #endregion

        #region Atualizar

        [Fact]
        public async Task AtualizarComEnderecoTelefone_Nao_OK_Quando_Registro_Nao_Existe()
        {
            var person = _faker.Person;
            var id = Guid.NewGuid();

            var telsNovo = new List<Telefone> {
                new Telefone("+55","11","90000-0000",id),
                new Telefone("+55","11","80000-0000", id)
            };

            var enderecoNovo = new Endereco("Rua", 11, "00000-000", "apt", "bairro", "cidade", "sp", TipoEnderecoEnum.Residencial, id);
            var funcionarioAtualizado = new Funcionario(id.ToString(), "matricular", "cargo 2",
                new Nome(person.FirstName, "Silva Sauro"),
                new DataNascimento(new DateTime(1985, 08, 14)),
                new Email(person.Email), telsNovo, enderecoNovo, enderecoNovo);

            _mockFuncionarioRepositorioEscrita.Setup(x => x.IniciarTransacao())
                .Returns(_mockTransacao.Object);

            _mockFuncionarioRepositorioLeitura.Setup(x => x.ObterPorId(It.IsAny<IDbTransaction>(), It.IsAny<Guid>()))
                .ReturnsAsync((Funcionario)null)
                .Callback<IDbTransaction, Guid>((transacao, id) =>
                {
                    Output.WriteLine($"Callback Email: {id}");
                    Assert.Equal(funcionarioAtualizado.Id, id);
                });

            var service = new FuncionarioService(_mockFuncionarioRepositorioLeitura.Object, _mockFuncionarioRepositorioEscrita.Object, _mockLogger.Object);

            await Assert.ThrowsAsync<NullReferenceException>(async () => await service.Atualizar(funcionarioAtualizado));

            Output.WriteLine($"Result: ok");

            _mockFuncionarioRepositorioLeitura.Verify(x => x.ObterPorId(It.IsAny<IDbTransaction>(), It.IsAny<Guid>()), Times.Once);
            _mockFuncionarioRepositorioEscrita.Verify(x => x.Atualizar(It.IsAny<Funcionario>(), It.IsAny<IDbTransaction>()), Times.Never);
            _mockFuncionarioRepositorioEscrita.Verify(x => x.IniciarTransacao(), Times.Once);
        }


        [Fact]
        public async Task AtualizarComEnderecoTelefone_Nao_OK_Quando_Registro_Nao_Atualizado()
        {
            var person = _faker.Person;
            var id = Guid.NewGuid();

            var telsNovo = new List<Telefone> {
                new Telefone("+55","11","90000-0000",id),
                new Telefone("+55","11","80000-0000", id)
            };

            var enderecoNovo = new Endereco("Rua", 11, "00000-000", "apt", "bairro", "cidade", "sp", TipoEnderecoEnum.Residencial, id);
            var funcionarioAtualizado = new Funcionario(id.ToString(), "matricular", "cargo 2",
                new Nome(person.FirstName, "Silva Sauro"),
                new DataNascimento(new DateTime(1985, 08, 14)),
                new Email(person.Email), telsNovo, enderecoNovo, enderecoNovo);

            _mockFuncionarioRepositorioEscrita.Setup(x => x.IniciarTransacao())
                .Returns(_mockTransacao.Object);

            _mockFuncionarioRepositorioEscrita.Setup(x => x.CancelarTransacao(It.IsAny<IDbTransaction>()));

            _mockFuncionarioRepositorioLeitura.Setup(x => x.ObterPorId(It.IsAny<IDbTransaction>(), It.IsAny<Guid>()))
                .ReturnsAsync(funcionarioAtualizado)
                .Callback<IDbTransaction, Guid>((transacao, id) =>
                {
                    Output.WriteLine($"Callback Email: {id}");
                    Assert.Equal(funcionarioAtualizado.Id, id);
                });

            _mockFuncionarioRepositorioEscrita.Setup(x => x.Atualizar(It.IsAny<Funcionario>(), It.IsAny<IDbTransaction>()))
                .ReturnsAsync(false)
                .Callback<Funcionario, IDbTransaction>((funcionario, transac) =>
                {
                    funcionario.Nome.Should().Be(funcionarioAtualizado.Nome);
                    funcionario.DataNascimento.Should().Be(funcionarioAtualizado.DataNascimento);
                    funcionario.Cargo.Should().Be(funcionarioAtualizado.Cargo);
                    funcionario.Matricula.Should().Be(funcionarioAtualizado.Matricula);
                });

            var service = new FuncionarioService(_mockFuncionarioRepositorioLeitura.Object, _mockFuncionarioRepositorioEscrita.Object, _mockLogger.Object);

            await service.Atualizar(funcionarioAtualizado);

            Output.WriteLine($"Result: ok");
            _mockFuncionarioRepositorioLeitura.Verify(x => x.ObterPorId(It.IsAny<IDbTransaction>(), It.IsAny<Guid>()), Times.Once);
            _mockFuncionarioRepositorioEscrita.Verify(x => x.Atualizar(It.IsAny<Funcionario>(), It.IsAny<IDbTransaction>()), Times.Once);
            _mockFuncionarioRepositorioEscrita.Verify(x => x.IniciarTransacao(), Times.Once);
            _mockFuncionarioRepositorioEscrita.Verify(x => x.CancelarTransacao(It.IsAny<IDbTransaction>()), Times.Once);
        }



        [Fact]
        public async Task AtualizarComEnderecoTelefone_OK_Quando_Registro_Existe()
        {
            var person = _faker.Person;
            var funcionarioId = Guid.NewGuid();

            var telsNovo = new List<Telefone> {
                new Telefone("+55","11","90000-0000", funcionarioId),
                new Telefone(2,"+55","11","80000-0000", funcionarioId)
            };
            var enderecoNovoResidencial = new Endereco(1, "Rua", 11, "00000-000", "apt", "bairro", "cidade", "sp", TipoEnderecoEnum.Comercial, funcionarioId);

            var enderecoNovoComercial = new Endereco("Rua", 11, "00000-000", "apt", "bairro", "cidade", "sp", TipoEnderecoEnum.Comercial, funcionarioId);
            var funcionarioAtualizado = new Funcionario(funcionarioId.ToString(), "matricular", "cargo 2",
                new Nome(person.FirstName, "Silva Sauro"),
                new DataNascimento(new DateTime(1985, 08, 14)),
                new Email(person.Email), telsNovo, enderecoNovoResidencial, enderecoNovoComercial);

            _mockFuncionarioRepositorioEscrita.Setup(x => x.IniciarTransacao())
                .Returns(_mockTransacao.Object);


            _mockFuncionarioRepositorioEscrita.Setup(x => x.Atualizar(It.IsAny<Funcionario>(), It.IsAny<IDbTransaction>()))
                .ReturnsAsync(true)
                .Callback<Funcionario, IDbTransaction>((funcionario, transac) =>
                {
                    funcionario.Nome.Should().Be(funcionarioAtualizado.Nome);
                    funcionario.DataNascimento.Should().Be(funcionarioAtualizado.DataNascimento);
                    funcionario.Cargo.Should().Be(funcionarioAtualizado.Cargo);
                    funcionario.Matricula.Should().Be(funcionarioAtualizado.Matricula);
                });


            _mockFuncionarioRepositorioEscrita.Setup(x => x.AtualizarEndereco(It.IsAny<Endereco>(), It.IsAny<IDbTransaction>()))
                .ReturnsAsync(true)
                .Callback<Endereco, IDbTransaction>((endereco, transac) =>
                {
                    endereco.Should().Be(enderecoNovoResidencial);
                });

            _mockFuncionarioRepositorioEscrita.Setup(x => x.InserirEndereco(It.IsAny<Endereco>(), It.IsAny<IDbTransaction>()))
                .ReturnsAsync(true)
                .Callback<Endereco, IDbTransaction>((endereco, transac) =>
                {
                    endereco.Should().Be(enderecoNovoComercial);
                });

            _mockFuncionarioRepositorioEscrita.Setup(x => x.AtualizarTelefone(It.IsAny<Telefone>(), It.IsAny<IDbTransaction>()))
                .ReturnsAsync(true)
                .Callback<Telefone, IDbTransaction>((telefone, transac) =>
                {
                    telefone.Should().Be(telsNovo.First(x => x.Id == 2));
                });

            _mockFuncionarioRepositorioEscrita.Setup(x => x.InserirTelefone(It.IsAny<Telefone>(), It.IsAny<IDbTransaction>()))
                .ReturnsAsync(true)
                .Callback<Telefone, IDbTransaction>((telefone, transac) =>
                {
                    telefone.Should().Be(telsNovo.First(x => x.Id == 0));
                });

            _mockFuncionarioRepositorioLeitura.Setup(x => x.ObterPorId(It.IsAny<IDbTransaction>(), It.IsAny<Guid>()))
                .ReturnsAsync(new Funcionario(funcionarioId.ToString(), "matricular", "cargo",
                new Nome(person.FirstName, "Silva"),
                new DataNascimento(new DateTime(1987, 08, 14)),
                new Email(person.Email), telsNovo, enderecoNovoResidencial, null))
                .Callback<IDbTransaction, Guid>((transacao, id) =>
                {
                    Output.WriteLine($"Callback ObterPorId: {id}");
                });

            var service = new FuncionarioService(_mockFuncionarioRepositorioLeitura.Object, _mockFuncionarioRepositorioEscrita.Object, _mockLogger.Object);

            await service.Atualizar(funcionarioAtualizado);

            Output.WriteLine($"Result: ok");

            _mockFuncionarioRepositorioLeitura.Verify(x => x.ObterPorId(It.IsAny<IDbTransaction>(), It.IsAny<Guid>()), Times.Once);
            _mockFuncionarioRepositorioEscrita.Verify(x => x.IniciarTransacao(), Times.Once);
            _mockFuncionarioRepositorioEscrita.Verify(x => x.AtualizarEndereco(It.IsAny<Endereco>(), It.IsAny<IDbTransaction>()), Times.Once);
            _mockFuncionarioRepositorioEscrita.Verify(x => x.InserirEndereco(It.IsAny<Endereco>(), It.IsAny<IDbTransaction>()), Times.Once);
            _mockFuncionarioRepositorioEscrita.Verify(x => x.AtualizarTelefone(It.IsAny<Telefone>(), It.IsAny<IDbTransaction>()), Times.Once);
            _mockFuncionarioRepositorioEscrita.Verify(x => x.InserirTelefone(It.IsAny<Telefone>(), It.IsAny<IDbTransaction>()), Times.Once);

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
            var endereco = new Endereco("Rua", 10, "00000-000", "apto", "bairro", "cidade", "sp", TipoEnderecoEnum.Residencial, id);
            var funcionario = new Funcionario(id.ToString(), "matricular", "cargo",
                new Nome(person.FirstName, person.LastName),
                new DataNascimento(new System.DateTime(1987, 08, 14)),
                new Email(person.Email), tels, endereco, endereco);

            _mockFuncionarioRepositorioEscrita.Setup(x => x.IniciarTransacao())
                .Returns(_mockTransacao.Object);

            _mockFuncionarioRepositorioLeitura.Setup(x => x.ObterPorId(It.IsAny<IDbTransaction>(), It.IsAny<Guid>()))
                .ReturnsAsync(funcionario)
                .Callback<IDbTransaction, Guid>((transacao, id) =>
                {
                    Output.WriteLine($"Callback Email: {id}");
                    Assert.Equal(funcionario.Id, id);
                });

            var service = new FuncionarioService(_mockFuncionarioRepositorioLeitura.Object, _mockFuncionarioRepositorioEscrita.Object, _mockLogger.Object);

            var result = await service.ObterPorId(funcionario.Id);

            Output.WriteLine($"Result: {result}");

            _mockFuncionarioRepositorioLeitura.Verify(x => x.ObterPorId(It.IsAny<IDbTransaction>(), It.IsAny<Guid>()), Times.Once);
            result.Should().NotBeNull();
        }

        [Fact]
        public async Task ObterPorId_OK_Quando_Registro_Nao_Existe()
        {
            _mockFuncionarioRepositorioLeitura.Setup(x => x.ObterPorId(It.IsAny<IDbTransaction>(), It.IsAny<Guid>()))
                .ReturnsAsync((Funcionario)null)
                .Callback<IDbTransaction, Guid>((transacao, id) =>
                {
                    Output.WriteLine($"Callback Email: {id}");
                });

            var service = new FuncionarioService(_mockFuncionarioRepositorioLeitura.Object, _mockFuncionarioRepositorioEscrita.Object, _mockLogger.Object);

            var result = await service.ObterPorId(Guid.NewGuid());

            Output.WriteLine($"Result: {result}");

            _mockFuncionarioRepositorioLeitura.Verify(x => x.ObterPorId(It.IsAny<IDbTransaction>(), It.IsAny<Guid>()), Times.Once);
            result.Should().BeNull();
        }

        [Fact]
        public async Task ObterPorId_Nao_OK_Quando_Base_Nao_Disponivel()
        {
            _mockFuncionarioRepositorioLeitura.Setup(x => x.ObterPorId(It.IsAny<IDbTransaction>(), It.IsAny<Guid>()))
                .Throws(new Exception("Teste"));

            var service = new FuncionarioService(_mockFuncionarioRepositorioLeitura.Object, _mockFuncionarioRepositorioEscrita.Object, _mockLogger.Object);

            await Assert.ThrowsAsync<Exception>(async () => await service.ObterPorId(Guid.NewGuid()));

            _mockFuncionarioRepositorioLeitura.Verify(x => x.ObterPorId(It.IsAny<IDbTransaction>(), It.IsAny<Guid>()), Times.Once);
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
            var endereco = new Endereco("Rua", 10, "00000-000", "apto", "bairro", "cidade", "sp", TipoEnderecoEnum.Residencial, id);
            var funcionario = new Funcionario(id.ToString(), "matricular", "cargo",
                new Nome(person.FirstName, person.LastName),
                new DataNascimento(new System.DateTime(1987, 08, 14)),
                new Email(person.Email), tels, endereco, endereco);

            _mockFuncionarioRepositorioLeitura.Setup(x => x.ObterTodos(It.IsAny<IDbTransaction>()))
                .ReturnsAsync(new List<Funcionario>() { funcionario });

            var service = new FuncionarioService(_mockFuncionarioRepositorioLeitura.Object, _mockFuncionarioRepositorioEscrita.Object, _mockLogger.Object);

            var result = await service.ObterTodos();

            Output.WriteLine($"Result: {result}");

            _mockFuncionarioRepositorioLeitura.Verify(x => x.ObterTodos(It.IsAny<IDbTransaction>()), Times.Once);
            result.Should().NotBeNull();
            result.Should().HaveCount(1);
        }

        [Fact]
        public async Task ObterPorTodos_OK_Quando_Registros_Nao_Existem()
        {
            _mockFuncionarioRepositorioLeitura.Setup(x => x.ObterTodos(It.IsAny<IDbTransaction>()))
                .ReturnsAsync(new List<Funcionario>());

            var service = new FuncionarioService(_mockFuncionarioRepositorioLeitura.Object, _mockFuncionarioRepositorioEscrita.Object, _mockLogger.Object);

            var result = await service.ObterTodos();

            Output.WriteLine($"Result: {result}");

            _mockFuncionarioRepositorioLeitura.Verify(x => x.ObterTodos(It.IsAny<IDbTransaction>()), Times.Once);
            result.Should().NotBeNull();
            result.Should().HaveCount(0);
        }

        [Fact]
        public async Task ObterPorTodos_Nao_OK_Quando_Base_Nao_Disponivel()
        {
            _mockFuncionarioRepositorioLeitura.Setup(x => x.ObterTodos(It.IsAny<IDbTransaction>()))
                .Throws(new Exception("Teste"));

            var service = new FuncionarioService(_mockFuncionarioRepositorioLeitura.Object, _mockFuncionarioRepositorioEscrita.Object, _mockLogger.Object);

            await Assert.ThrowsAsync<Exception>(async () => await service.ObterTodos());

            _mockFuncionarioRepositorioLeitura.Verify(x => x.ObterTodos(It.IsAny<IDbTransaction>()), Times.Once);
        }
        #endregion
    }
}