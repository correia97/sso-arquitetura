using Cadastro.Domain.Entities;
using Cadastro.Domain.Enums;
using Cadastro.Domain.Interfaces;
using Cadastro.Domain.Services;
using Domain.Entities;
using Domain.ValueObject;
using Microsoft.Extensions.Logging;
using System.Data;
using System.Diagnostics;
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
        public readonly Mock<INotificationService> _mockNotificationService;
        public readonly Faker _faker;
        public readonly Mock<ILogger<FuncionarioService>> _mockLogger;
        private ITestOutputHelper Output { get; }

        public FuncionarioServiceTest(ITestOutputHelper outputHelper)
        {
            _mockFuncionarioRepositorioLeitura = new Mock<IFuncionarioReadRepository>();
            _mockFuncionarioRepositorioEscrita = new Mock<IFuncionarioWriteRepository>();
            _mockNotificationService = new Mock<INotificationService>();
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
            var activity = new ActivitySource("Cadastrar_OK_Quando_EMail_Nao_Existe");
            var person = _faker.Person;
            var funcionario = new Funcionario(Guid.NewGuid().ToString(), "matricular", "cargo",
                new Nome(person.FirstName, person.LastName),
                new DataNascimento(new System.DateTime(1987, 08, 14)),
                new Email(person.Email));

            _mockFuncionarioRepositorioEscrita.Setup(x => x.IniciarTransacao());

            _mockFuncionarioRepositorioEscrita.Setup(x => x.Inserir(It.IsAny<Funcionario>()))
                .ReturnsAsync(true);

            _mockFuncionarioRepositorioLeitura.Setup(x => x.ObterPorEmail(It.IsAny<string>()))
                .ReturnsAsync((Funcionario)null)
                .Callback<string>((email) =>
                {
                    Output.WriteLine($"Callback Email: {email}");
                    Assert.Equal(funcionario.Email.EnderecoEmail, email);
                });

            var service = new FuncionarioService(_mockFuncionarioRepositorioLeitura.Object, _mockFuncionarioRepositorioEscrita.Object,
                                                  _mockNotificationService.Object, _mockLogger.Object, activity);

            await service.Cadastrar(funcionario);

            Output.WriteLine($"Result: ok");

            _mockFuncionarioRepositorioLeitura.Verify(x => x.ObterPorEmail(It.IsAny<string>()), Times.Once);
            _mockFuncionarioRepositorioEscrita.Verify(x => x.Inserir(It.IsAny<Funcionario>()), Times.Once);
        }

        [Fact]
        public async Task Cadastrar_Nao_OK_Quando_EMail_Ja_Existe()
        {
            var activity = new ActivitySource("Cadastrar_Nao_OK_Quando_EMail_Ja_Existe");
            var person = _faker.Person;
            var funcionario = new Funcionario(Guid.NewGuid().ToString(), "matricular", "cargo",
                new Nome(person.FirstName, person.LastName),
                new DataNascimento(new System.DateTime(1987, 08, 14)),
                new Email(person.Email));

            _mockFuncionarioRepositorioEscrita.Setup(x => x.IniciarTransacao());

            _mockFuncionarioRepositorioLeitura.Setup(x => x.ObterPorEmail(It.IsAny<string>()))
                .ReturnsAsync(funcionario)
                .Callback<string>((email) =>
                {
                    Output.WriteLine($"Callback Email: {email}");
                    Assert.Equal(funcionario.Email.EnderecoEmail, email);
                });

            var service = new FuncionarioService(_mockFuncionarioRepositorioLeitura.Object, _mockFuncionarioRepositorioEscrita.Object,
                 _mockNotificationService.Object, _mockLogger.Object, activity);

            await Assert.ThrowsAsync<InvalidOperationException>(async () => await service.Cadastrar(funcionario));

            Output.WriteLine($"Result: ok");

            _mockFuncionarioRepositorioLeitura.Verify(x => x.ObterPorEmail(It.IsAny<string>()), Times.Once);
            _mockFuncionarioRepositorioEscrita.Verify(x => x.Inserir(It.IsAny<Funcionario>()), Times.Never);
        }

        [Fact]
        public async Task Cadastrar_Nao_OK_Quando_Banco_Nao_Acessivel()
        {
            var activity = new ActivitySource("Cadastrar_Nao_OK_Quando_Banco_Nao_Acessivel");
            var person = _faker.Person;
            var funcionario = new Funcionario(Guid.NewGuid().ToString(), "matricular", "cargo",
                new Nome(person.FirstName, person.LastName),
                new DataNascimento(new System.DateTime(1987, 08, 14)),
                new Email(person.Email));

            _mockFuncionarioRepositorioEscrita.Setup(x => x.IniciarTransacao());

            _mockFuncionarioRepositorioLeitura.Setup(x => x.ObterPorEmail(It.IsAny<string>()))
                .Throws(new Exception());

            var service = new FuncionarioService(_mockFuncionarioRepositorioLeitura.Object, _mockFuncionarioRepositorioEscrita.Object,
                _mockNotificationService.Object, _mockLogger.Object, activity);

            await Assert.ThrowsAsync<Exception>(async () => await service.Cadastrar(funcionario));

            Output.WriteLine($"Result: ok");

            _mockFuncionarioRepositorioLeitura.Verify(x => x.ObterPorEmail(It.IsAny<string>()), Times.Once);
            _mockFuncionarioRepositorioEscrita.Verify(x => x.IniciarTransacao(), Times.Never);
        }

        [Fact]
        public async Task CadastrarComEnderecoTelefone_OK_Quando_EMail_Nao_Existe()
        {
            var activity = new ActivitySource("CadastrarComEnderecoTelefone_OK_Quando_EMail_Nao_Existe");
            var person = _faker.Person;
            var id = Guid.NewGuid();
            var tels = new List<Telefone> {
                new Telefone("+55","11","90000-0000", id)
            };
            var endereco = new Endereco("Rua", 10, "00000-000", "apto", "bairro", "cidade", "sp", TipoEndereco.Residencial, id);
            var funcionario = new Funcionario(id.ToString(), "matricular", "cargo",
                new Nome(person.FirstName, person.LastName),
                new DataNascimento(new System.DateTime(1987, 08, 14)),
                new Email(person.Email), tels, endereco, endereco);

            _mockFuncionarioRepositorioEscrita.Setup(x => x.IniciarTransacao());

            _mockFuncionarioRepositorioEscrita.Setup(x => x.Inserir(It.IsAny<Funcionario>()))
                .ReturnsAsync(true);

            _mockFuncionarioRepositorioLeitura.Setup(x => x.ObterPorEmail(It.IsAny<string>()))
                .ReturnsAsync((Funcionario)null)
                .Callback<string>((email) =>
                {
                    Output.WriteLine($"Callback Email: {email}");
                    Assert.Equal(funcionario.Email.EnderecoEmail, email);
                });

            var service = new FuncionarioService(_mockFuncionarioRepositorioLeitura.Object, _mockFuncionarioRepositorioEscrita.Object,
                                                 _mockNotificationService.Object, _mockLogger.Object, activity);

            await service.Cadastrar(funcionario);

            Output.WriteLine($"Result: ok");

            _mockFuncionarioRepositorioLeitura.Verify(x => x.ObterPorEmail(It.IsAny<string>()), Times.Once);
            _mockFuncionarioRepositorioEscrita.Verify(x => x.Inserir(It.IsAny<Funcionario>()), Times.Once);
            _mockFuncionarioRepositorioEscrita.Verify(x => x.IniciarTransacao(), Times.Once);
        }

        [Fact]
        public async Task CadastrarComEnderecoTelefone_Nao_OK_Quando_EMail_Ja_Existe()
        {
            var activity = new ActivitySource("CadastrarComEnderecoTelefone_Nao_OK_Quando_EMail_Ja_Existe");
            var person = _faker.Person;
            var id = Guid.NewGuid();
            var tels = new List<Telefone> {
                new Telefone("+55","11","90000-0000",id)
            };
            var endereco = new Endereco("Rua", 10, "00000-000", "apto", "bairro", "cidade", "sp", TipoEndereco.Comercial, id);
            var funcionario = new Funcionario(id.ToString(), "matricular", "cargo",
                new Nome(person.FirstName, person.LastName),
                new DataNascimento(new DateTime(1987, 08, 14)),
                new Email(person.Email), tels, endereco, endereco);

            _mockFuncionarioRepositorioEscrita.Setup(x => x.IniciarTransacao());

            _mockFuncionarioRepositorioLeitura.Setup(x => x.ObterPorEmail(It.IsAny<string>()))
                .ReturnsAsync(funcionario)
                .Callback<string>((email) =>
                {
                    Output.WriteLine($"Callback Email: {email}");
                    Assert.Equal(funcionario.Email.EnderecoEmail, email);
                });

            var service = new FuncionarioService(_mockFuncionarioRepositorioLeitura.Object, _mockFuncionarioRepositorioEscrita.Object,
                                                  _mockNotificationService.Object, _mockLogger.Object, activity);

            await Assert.ThrowsAsync<InvalidOperationException>(async () => await service.Cadastrar(funcionario));

            Output.WriteLine($"Result: ok");

            _mockFuncionarioRepositorioLeitura.Verify(x => x.ObterPorEmail(It.IsAny<string>()), Times.Once);
            _mockFuncionarioRepositorioEscrita.Verify(x => x.IniciarTransacao(), Times.Never);
            _mockFuncionarioRepositorioEscrita.Verify(x => x.Inserir(It.IsAny<Funcionario>()), Times.Never);
        }


        [Fact]
        public async Task CadastrarComEnderecoTelefone_Nao_OK_Quando_Erro_Inserir()
        {
            var activity = new ActivitySource("CadastrarComEnderecoTelefone_Nao_OK_Quando_Erro_Inserir");
            var person = _faker.Person;
            var id = Guid.NewGuid();
            var tels = new List<Telefone> {
                new Telefone("+55","11","90000-0000",id)
            };
            var endereco = new Endereco("Rua", 10, "00000-000", "apto", "bairro", "cidade", "sp", TipoEndereco.Comercial, id);
            var funcionario = new Funcionario(id.ToString(), "matricular", "cargo",
                new Nome(person.FirstName, person.LastName),
                new DataNascimento(new DateTime(1987, 08, 14)),
                new Email(person.Email), tels, endereco, endereco);

            _mockFuncionarioRepositorioEscrita.Setup(x => x.IniciarTransacao());

            _mockFuncionarioRepositorioEscrita.Setup(x => x.CancelarTransacao());

            _mockFuncionarioRepositorioLeitura.Setup(x => x.ObterPorEmail(It.IsAny<string>()))
                .ReturnsAsync((Funcionario)null)
                .Callback<string>((email) =>
                {
                    Output.WriteLine($"Callback Email: {email}");
                    Assert.Equal(funcionario.Email.EnderecoEmail, email);
                });

            _mockFuncionarioRepositorioEscrita.Setup(x => x.Inserir(It.IsAny<Funcionario>()))
                                                .ReturnsAsync(false);

            var service = new FuncionarioService(_mockFuncionarioRepositorioLeitura.Object, _mockFuncionarioRepositorioEscrita.Object,
                                                 _mockNotificationService.Object, _mockLogger.Object, activity);

            await Assert.ThrowsAsync<InvalidOperationException>(async () => await service.Cadastrar(funcionario));

            Output.WriteLine($"Result: ok");

            _mockFuncionarioRepositorioLeitura.Verify(x => x.ObterPorEmail(It.IsAny<string>()), Times.Once);
            _mockFuncionarioRepositorioEscrita.Verify(x => x.Inserir(It.IsAny<Funcionario>()), Times.Once);
            _mockFuncionarioRepositorioEscrita.Verify(x => x.IniciarTransacao(), Times.Once);
            _mockFuncionarioRepositorioEscrita.Verify(x => x.CancelarTransacao(), Times.Once);
        }

        #endregion

        #region Atualizar

        [Fact]
        public async Task AtualizarComEnderecoTelefone_Nao_OK_Quando_Registro_Nao_Existe()
        {
            var activity = new ActivitySource("AtualizarComEnderecoTelefone_Nao_OK_Quando_Registro_Nao_Existe");
            var person = _faker.Person;
            var id = Guid.NewGuid();

            var telsNovo = new List<Telefone> {
                new Telefone("+55","11","90000-0000",id),
                new Telefone("+55","11","80000-0000", id)
            };

            var enderecoNovo = new Endereco("Rua", 11, "00000-000", "apt", "bairro", "cidade", "sp", TipoEndereco.Residencial, id);
            var funcionarioAtualizado = new Funcionario(id.ToString(), "matricular", "cargo 2",
                new Nome(person.FirstName, "Silva Sauro"),
                new DataNascimento(new DateTime(1985, 08, 14)),
                new Email(person.Email), telsNovo, enderecoNovo, enderecoNovo);

            _mockFuncionarioRepositorioEscrita.Setup(x => x.IniciarTransacao());

            _mockFuncionarioRepositorioLeitura.Setup(x => x.ObterPorId(It.IsAny<Guid>()))
                .ReturnsAsync((Funcionario)null)
                .Callback<Guid>((id) =>
                {
                    Output.WriteLine($"Callback Email: {id}");
                    Assert.Equal(funcionarioAtualizado.Id, id);
                });

            var service = new FuncionarioService(_mockFuncionarioRepositorioLeitura.Object, _mockFuncionarioRepositorioEscrita.Object,
                                                 _mockNotificationService.Object, _mockLogger.Object, activity);

            await Assert.ThrowsAsync<InvalidOperationException>(async () => await service.Atualizar(funcionarioAtualizado));

            Output.WriteLine($"Result: ok");

            _mockFuncionarioRepositorioLeitura.Verify(x => x.ObterPorId(It.IsAny<Guid>()), Times.Once);
            _mockFuncionarioRepositorioEscrita.Verify(x => x.Atualizar(It.IsAny<Funcionario>()), Times.Never);
            _mockFuncionarioRepositorioEscrita.Verify(x => x.IniciarTransacao(), Times.Never);
        }

        [Fact]
        public async Task AtualizarComEnderecoTelefone_Nao_OK_Quando_Registro_Nao_Atualizado()
        {
            var activity = new ActivitySource("AtualizarComEnderecoTelefone_Nao_OK_Quando_Registro_Nao_Atualizado");
            var person = _faker.Person;
            var id = Guid.NewGuid();

            var telsNovo = new List<Telefone> {
                new Telefone("+55","11","90000-0000",id),
                new Telefone("+55","11","80000-0000", id)
            };

            var enderecoNovo = new Endereco("Rua", 11, "00000-000", "apt", "bairro", "cidade", "sp", TipoEndereco.Residencial, id);
            var funcionarioAtualizado = new Funcionario(id.ToString(), "matricular", "cargo 2",
                new Nome(person.FirstName, "Silva Sauro"),
                new DataNascimento(new DateTime(1985, 08, 14)),
                new Email(person.Email), telsNovo, enderecoNovo, enderecoNovo);

            _mockFuncionarioRepositorioEscrita.Setup(x => x.IniciarTransacao());

            _mockFuncionarioRepositorioEscrita.Setup(x => x.CancelarTransacao());

            _mockFuncionarioRepositorioLeitura.Setup(x => x.ObterPorId(It.IsAny<Guid>()))
                .ReturnsAsync(funcionarioAtualizado)
                .Callback<Guid>((id) =>
                {
                    Output.WriteLine($"Callback Email: {id}");
                    Assert.Equal(funcionarioAtualizado.Id, id);
                });

            _mockFuncionarioRepositorioEscrita.Setup(x => x.Atualizar(It.IsAny<Funcionario>()))
                .ReturnsAsync(false)
                .Callback<Funcionario>((funcionario) =>
                {
                    funcionario.Nome.Should().Be(funcionarioAtualizado.Nome);
                    funcionario.DataNascimento.Should().Be(funcionarioAtualizado.DataNascimento);
                    funcionario.Cargo.Should().Be(funcionarioAtualizado.Cargo);
                    funcionario.Matricula.Should().Be(funcionarioAtualizado.Matricula);
                });

            var service = new FuncionarioService(_mockFuncionarioRepositorioLeitura.Object, _mockFuncionarioRepositorioEscrita.Object,
                                               _mockNotificationService.Object, _mockLogger.Object, activity);

            await Assert.ThrowsAsync<InvalidOperationException>(() => service.Atualizar(funcionarioAtualizado));

            Output.WriteLine($"Result: ok");
            _mockFuncionarioRepositorioLeitura.Verify(x => x.ObterPorId(It.IsAny<Guid>()), Times.Once);
            _mockFuncionarioRepositorioEscrita.Verify(x => x.Atualizar(It.IsAny<Funcionario>()), Times.Once);
            _mockFuncionarioRepositorioEscrita.Verify(x => x.IniciarTransacao(), Times.Once);
            _mockFuncionarioRepositorioEscrita.Verify(x => x.CancelarTransacao(), Times.Once);
        }

        [Fact]
        public async Task AtualizarComEnderecoTelefone_OK_Quando_Registro_Existe()
        {
            var activity = new ActivitySource("AtualizarComEnderecoTelefone_OK_Quando_Registro_Existe");
            var person = _faker.Person;
            var funcionarioId = Guid.NewGuid();

            var telsNovo = new List<Telefone> {
                new Telefone("+55","11","90000-0000", funcionarioId),
                new Telefone(2,"+55","11","80000-0000", funcionarioId)
            };
            var enderecoNovoResidencial = new Endereco(1, "Rua", 11, "00000-000", "apt", "bairro", "cidade", "sp", TipoEndereco.Comercial, funcionarioId);

            var enderecoNovoComercial = new Endereco("Rua", 11, "00000-000", "apt", "bairro", "cidade", "sp", TipoEndereco.Comercial, funcionarioId);
            var funcionarioAtualizado = new Funcionario(funcionarioId.ToString(), "matricular", "cargo 2",
                new Nome(person.FirstName, "Silva Sauro"),
                new DataNascimento(new DateTime(1985, 08, 14)),
                new Email(person.Email), telsNovo, enderecoNovoResidencial, enderecoNovoComercial);

            _mockFuncionarioRepositorioEscrita.Setup(x => x.IniciarTransacao());

            _mockFuncionarioRepositorioEscrita.Setup(x => x.Atualizar(It.IsAny<Funcionario>()))
                .ReturnsAsync(true)
                .Callback<Funcionario>((funcionario) =>
                {
                    funcionario.Nome.Should().Be(funcionarioAtualizado.Nome);
                    funcionario.DataNascimento.Should().Be(funcionarioAtualizado.DataNascimento);
                    funcionario.Cargo.Should().Be(funcionarioAtualizado.Cargo);
                    funcionario.Matricula.Should().Be(funcionarioAtualizado.Matricula);
                });


            _mockFuncionarioRepositorioEscrita.Setup(x => x.AtualizarEndereco(It.IsAny<Endereco>()))
                .ReturnsAsync(true)
                .Callback<Endereco>((endereco) =>
                {
                    endereco.Should().Be(enderecoNovoResidencial);
                });

            _mockFuncionarioRepositorioEscrita.Setup(x => x.InserirEndereco(It.IsAny<Endereco>()))
                .ReturnsAsync(true)
                .Callback<Endereco>((endereco) =>
                {
                    endereco.Should().Be(enderecoNovoComercial);
                });

            _mockFuncionarioRepositorioEscrita.Setup(x => x.AtualizarTelefone(It.IsAny<Telefone>()))
                .ReturnsAsync(true)
                .Callback<Telefone>((telefone) =>
                {
                    telefone.Should().Be(telsNovo.First(x => x.Id == 2));
                });

            _mockFuncionarioRepositorioEscrita.Setup(x => x.InserirTelefone(It.IsAny<Telefone>()))
                .ReturnsAsync(true)
                .Callback<Telefone>((telefone) =>
                {
                    telefone.Should().Be(telsNovo.First(x => x.Id == 0));
                });

            _mockFuncionarioRepositorioLeitura.Setup(x => x.ObterPorId(It.IsAny<Guid>()))
                .ReturnsAsync(new Funcionario(funcionarioId.ToString(), "matricular", "cargo",
                new Nome(person.FirstName, "Silva"),
                new DataNascimento(new DateTime(1987, 08, 14)),
                new Email(person.Email), telsNovo, enderecoNovoResidencial, null))
                .Callback<Guid>((id) =>
                {
                    Output.WriteLine($"Callback ObterPorId: {id}");
                });

            var service = new FuncionarioService(_mockFuncionarioRepositorioLeitura.Object, _mockFuncionarioRepositorioEscrita.Object,
                                                _mockNotificationService.Object, _mockLogger.Object, activity);

            await service.Atualizar(funcionarioAtualizado);

            Output.WriteLine($"Result: ok");

            _mockFuncionarioRepositorioLeitura.Verify(x => x.ObterPorId(It.IsAny<Guid>()), Times.Once);
            _mockFuncionarioRepositorioEscrita.Verify(x => x.IniciarTransacao(), Times.Once);
            _mockFuncionarioRepositorioEscrita.Verify(x => x.AtualizarEndereco(It.IsAny<Endereco>()), Times.Once);
            _mockFuncionarioRepositorioEscrita.Verify(x => x.InserirEndereco(It.IsAny<Endereco>()), Times.Once);
            _mockFuncionarioRepositorioEscrita.Verify(x => x.AtualizarTelefone(It.IsAny<Telefone>()), Times.Once);
            _mockFuncionarioRepositorioEscrita.Verify(x => x.InserirTelefone(It.IsAny<Telefone>()), Times.Once);

        }
        #endregion

        #region Desativar

        [Fact]
        public async Task Desativar_Nao_OK_Quando_Registro_Nao_Alterado()
        {
            var activity = new ActivitySource("Desativar_Nao_OK_Quando_Registro_Nao_Alterado");

            var id = Guid.NewGuid();

            _mockFuncionarioRepositorioEscrita.Setup(x => x.IniciarTransacao());

            _mockFuncionarioRepositorioEscrita.Setup(x => x.CancelarTransacao());

            _mockFuncionarioRepositorioEscrita.Setup(x => x.Desativar(It.IsAny<Guid>()))
                .ReturnsAsync(false)
                .Callback<Guid>((currentId) =>
                {
                    Output.WriteLine($"Callback Desativar: {currentId}");
                    Assert.Equal(id, currentId);
                });

            var service = new FuncionarioService(_mockFuncionarioRepositorioLeitura.Object, _mockFuncionarioRepositorioEscrita.Object,
                                               _mockNotificationService.Object, _mockLogger.Object, activity);

            await Assert.ThrowsAsync<InvalidOperationException>(() => service.Desativar(id));

            Output.WriteLine($"Result: ok");
            _mockFuncionarioRepositorioEscrita.Verify(x => x.Desativar(It.IsAny<Guid>()), Times.Once);
            _mockFuncionarioRepositorioEscrita.Verify(x => x.IniciarTransacao(), Times.Once);
            _mockFuncionarioRepositorioEscrita.Verify(x => x.CancelarTransacao(), Times.Once);
        }

        [Fact]
        public async Task Desativar_OK_Quando_Registro_Existe()
        {
            var activity = new ActivitySource("Desativar_OK_Quando_Registro_Existe");

            var id = Guid.NewGuid();

            _mockFuncionarioRepositorioEscrita.Setup(x => x.IniciarTransacao());

            _mockFuncionarioRepositorioEscrita.Setup(x => x.CompletarTransacao());

            _mockFuncionarioRepositorioEscrita.Setup(x => x.Desativar(It.IsAny<Guid>()))
                .ReturnsAsync(true)
                .Callback<Guid>((currentId) =>
                {
                    Output.WriteLine($"Callback Desativar: {currentId}");
                    Assert.Equal(id, currentId);
                });

            var service = new FuncionarioService(_mockFuncionarioRepositorioLeitura.Object, _mockFuncionarioRepositorioEscrita.Object,
                                               _mockNotificationService.Object, _mockLogger.Object, activity);

            await service.Desativar(id);

            Output.WriteLine($"Result: ok");
            _mockFuncionarioRepositorioEscrita.Verify(x => x.Desativar(It.IsAny<Guid>()), Times.Once);
            _mockFuncionarioRepositorioEscrita.Verify(x => x.IniciarTransacao(), Times.Once);
            _mockFuncionarioRepositorioEscrita.Verify(x => x.CompletarTransacao(), Times.Once);
        }

        #endregion

        #region Remover

        [Fact]
        public async Task Remover_Nao_OK_Quando_Registro_Nao_Existe()
        {
            var activity = new ActivitySource("Remover_Nao_OK_Quando_Registro_Nao_Existe");
            var id = Guid.NewGuid();


            _mockFuncionarioRepositorioLeitura.Setup(x => x.ObterPorId(It.IsAny<Guid>()))
                .ReturnsAsync((Funcionario)null)
                .Callback<Guid>((currentId) =>
                {
                    Output.WriteLine($"Callback Email: {currentId}");
                    Assert.Equal(id, currentId);
                });

            _mockNotificationService.Setup(x => x.SendEvent(It.IsAny<NotificationMessage>()));

            var service = new FuncionarioService(_mockFuncionarioRepositorioLeitura.Object, _mockFuncionarioRepositorioEscrita.Object,
                                                 _mockNotificationService.Object, _mockLogger.Object, activity);

            await Assert.ThrowsAsync<InvalidOperationException>(async () => await service.Remover(id));

            Output.WriteLine($"Result: ok");

            _mockFuncionarioRepositorioLeitura.Verify(x => x.ObterPorId(It.IsAny<Guid>()), Times.Once);
            _mockNotificationService.Verify(x => x.SendEvent(It.IsAny<NotificationMessage>()), Times.Once);
            _mockFuncionarioRepositorioEscrita.Verify(x => x.IniciarTransacao(), Times.Never);
        }

        [Fact]
        public async Task Remover_Nao_OK_Quando_Registro_Nao_Removido()
        {
            var activity = new ActivitySource("Remover_Nao_OK_Quando_Registro_Nao_Removido");
            var person = _faker.Person;
            var id = Guid.NewGuid();

            var telsNovo = new List<Telefone> {
                new Telefone("+55","11","90000-0000",id),
                new Telefone("+55","11","80000-0000", id)
            };

            var enderecoNovo = new Endereco("Rua", 11, "00000-000", "apt", "bairro", "cidade", "sp", TipoEndereco.Residencial, id);
            var funcionarioAtualizado = new Funcionario(id.ToString(), "matricular", "cargo 2",
                new Nome(person.FirstName, "Silva Sauro"),
                new DataNascimento(new DateTime(1985, 08, 14)),
                new Email(person.Email), telsNovo, enderecoNovo, enderecoNovo);

            _mockFuncionarioRepositorioEscrita.Setup(x => x.IniciarTransacao());

            _mockFuncionarioRepositorioEscrita.Setup(x => x.CancelarTransacao());

            _mockFuncionarioRepositorioLeitura.Setup(x => x.ObterPorId(It.IsAny<Guid>()))
                .ReturnsAsync(funcionarioAtualizado)
                .Callback<Guid>((currentId) =>
                {
                    Output.WriteLine($"Callback Email: {currentId}");
                    Assert.Equal(funcionarioAtualizado.Id, currentId);
                });
            _mockFuncionarioRepositorioEscrita.Setup(x => x.RemoverEndereco(It.IsAny<int>())).ReturnsAsync(true);
            _mockFuncionarioRepositorioEscrita.Setup(x => x.RemoverTelefone(It.IsAny<int>())).ReturnsAsync(true);

            _mockFuncionarioRepositorioEscrita.Setup(x => x.Remover(It.IsAny<Guid>()))
                .ReturnsAsync(false)
                .Callback<Guid>((currentId) =>
                {
                    Output.WriteLine($"Callback Remover: {currentId}");
                    Assert.Equal(id, currentId);
                });

            var service = new FuncionarioService(_mockFuncionarioRepositorioLeitura.Object, _mockFuncionarioRepositorioEscrita.Object,
                                               _mockNotificationService.Object, _mockLogger.Object, activity);

            await Assert.ThrowsAsync<InvalidOperationException>(() => service.Remover(id));

            Output.WriteLine($"Result: ok");
            _mockFuncionarioRepositorioLeitura.Verify(x => x.ObterPorId(It.IsAny<Guid>()), Times.Once);
            _mockFuncionarioRepositorioEscrita.Verify(x => x.RemoverTelefone(It.IsAny<int>()), Times.Exactly(2));
            _mockFuncionarioRepositorioEscrita.Verify(x => x.RemoverEndereco(It.IsAny<int>()), Times.Exactly(2));
            _mockFuncionarioRepositorioEscrita.Verify(x => x.Remover(It.IsAny<Guid>()), Times.Once);
            _mockFuncionarioRepositorioEscrita.Verify(x => x.IniciarTransacao(), Times.Once);
            _mockFuncionarioRepositorioEscrita.Verify(x => x.CancelarTransacao(), Times.Once);
        }

        [Fact]
        public async Task Remover_OK_Quando_Registro_Existe()
        {
            var activity = new ActivitySource("Remover_OK_Quando_Registro_Existe");
            var person = _faker.Person;
            var id = Guid.NewGuid();

            var telsNovo = new List<Telefone> {
                new Telefone("+55","11","90000-0000",id),
                new Telefone("+55","11","80000-0000", id)
            };

            var enderecoNovo = new Endereco("Rua", 11, "00000-000", "apt", "bairro", "cidade", "sp", TipoEndereco.Residencial, id);
            var funcionarioAtualizado = new Funcionario(id.ToString(), "matricular", "cargo 2",
                new Nome(person.FirstName, "Silva Sauro"),
                new DataNascimento(new DateTime(1985, 08, 14)),
                new Email(person.Email), telsNovo, enderecoNovo, enderecoNovo);

            _mockFuncionarioRepositorioEscrita.Setup(x => x.IniciarTransacao());

            _mockFuncionarioRepositorioEscrita.Setup(x => x.CompletarTransacao());

            _mockFuncionarioRepositorioLeitura.Setup(x => x.ObterPorId(It.IsAny<Guid>()))
                .ReturnsAsync(funcionarioAtualizado)
                .Callback<Guid>((currentId) =>
                {
                    Output.WriteLine($"Callback Email: {currentId}");
                    Assert.Equal(funcionarioAtualizado.Id, currentId);
                });
            _mockFuncionarioRepositorioEscrita.Setup(x => x.RemoverEndereco(It.IsAny<int>())).ReturnsAsync(true);
            _mockFuncionarioRepositorioEscrita.Setup(x => x.RemoverTelefone(It.IsAny<int>())).ReturnsAsync(true);

            _mockFuncionarioRepositorioEscrita.Setup(x => x.Remover(It.IsAny<Guid>()))
                .ReturnsAsync(true)
                .Callback<Guid>((currentId) =>
                {
                    Output.WriteLine($"Callback Remover: {currentId}");
                    Assert.Equal(id, currentId);
                });

            var service = new FuncionarioService(_mockFuncionarioRepositorioLeitura.Object, _mockFuncionarioRepositorioEscrita.Object,
                                               _mockNotificationService.Object, _mockLogger.Object, activity);

            await service.Remover(id);

            Output.WriteLine($"Result: ok");
            _mockFuncionarioRepositorioLeitura.Verify(x => x.ObterPorId(It.IsAny<Guid>()), Times.Once);
            _mockFuncionarioRepositorioEscrita.Verify(x => x.RemoverTelefone(It.IsAny<int>()), Times.Exactly(2));
            _mockFuncionarioRepositorioEscrita.Verify(x => x.RemoverEndereco(It.IsAny<int>()), Times.Exactly(2));
            _mockFuncionarioRepositorioEscrita.Verify(x => x.Remover(It.IsAny<Guid>()), Times.Once);
            _mockFuncionarioRepositorioEscrita.Verify(x => x.IniciarTransacao(), Times.Once);
            _mockFuncionarioRepositorioEscrita.Verify(x => x.CompletarTransacao(), Times.Once);

        }
        #endregion

        #region ObterPorId

        [Fact]
        public async Task ObterPorId_OK_Quando_Registro_Existe()
        {
            var activity = new ActivitySource("ObterPorId_OK_Quando_Registro_Existe");
            var person = _faker.Person;
            var id = Guid.NewGuid();
            var tels = new List<Telefone> {
                new Telefone("+55","11","90000-0000", id)
            };
            var enderecoRes = new Endereco("Rua", 10, "00000-000", "apto", "bairro", "cidade", "sp", TipoEndereco.Residencial, id);
            var enderecoCom = new Endereco("Rua", 10, "00000-000", "apto", "bairro", "cidade", "sp", TipoEndereco.Comercial, id);
            var funcionario = new Funcionario(id.ToString(), "matricular", "cargo",
                new Nome(person.FirstName, person.LastName),
                new DataNascimento(new System.DateTime(1987, 08, 14)),
                new Email(person.Email), tels, enderecoRes, enderecoCom);

            _mockFuncionarioRepositorioLeitura.Setup(x => x.ObterPorId(It.IsAny<Guid>()))
                .ReturnsAsync(funcionario)
                .Callback<Guid>((id) =>
                {
                    Output.WriteLine($"Callback Email: {id}");
                    Assert.Equal(funcionario.Id, id);
                });

            _mockFuncionarioRepositorioLeitura.Setup(x => x.ObterEnderecosPorFuncionarioId(It.IsAny<Guid>()))
                .ReturnsAsync(new List<Endereco> { enderecoRes, enderecoCom })
                .Callback<Guid>((id) =>
                {
                    Output.WriteLine($"Callback Email: {id}");
                    Assert.Equal(funcionario.Id, id);
                });

            _mockFuncionarioRepositorioLeitura.Setup(x => x.ObterTelefonesPorFuncionarioId(It.IsAny<Guid>()))
                .ReturnsAsync(tels)
                .Callback<Guid>((id) =>
                {
                    Output.WriteLine($"Callback Email: {id}");
                    Assert.Equal(funcionario.Id, id);
                });

            var service = new FuncionarioService(_mockFuncionarioRepositorioLeitura.Object, _mockFuncionarioRepositorioEscrita.Object,
                                                _mockNotificationService.Object, _mockLogger.Object, activity);

            var result = await service.ObterPorId(funcionario.Id);

            Output.WriteLine($"Result: {result}");

            _mockFuncionarioRepositorioLeitura.Verify(x => x.ObterPorId(It.IsAny<Guid>()), Times.Once);

            _mockFuncionarioRepositorioLeitura.Verify(x => x.ObterEnderecosPorFuncionarioId(It.IsAny<Guid>()), Times.Once);

            _mockFuncionarioRepositorioLeitura.Verify(x => x.ObterTelefonesPorFuncionarioId(It.IsAny<Guid>()), Times.Once);
            result.Should().NotBeNull();
        }

        [Fact]
        public async Task ObterPorId_OK_Quando_Registro_Nao_Existe()
        {
            var activity = new ActivitySource("ObterPorId_OK_Quando_Registro_Nao_Existe");
            _mockFuncionarioRepositorioLeitura.Setup(x => x.ObterPorId(It.IsAny<Guid>()))
                .ReturnsAsync((Funcionario)null)
                .Callback<Guid>((id) =>
                {
                    Output.WriteLine($"Callback Email: {id}");
                });

            var service = new FuncionarioService(_mockFuncionarioRepositorioLeitura.Object, _mockFuncionarioRepositorioEscrita.Object,
                                                    _mockNotificationService.Object, _mockLogger.Object, activity);

            var result = await service.ObterPorId(Guid.NewGuid());

            Output.WriteLine($"Result: {result}");

            _mockFuncionarioRepositorioLeitura.Verify(x => x.ObterPorId(It.IsAny<Guid>()), Times.Once);
            result.Should().BeNull();
        }

        [Fact]
        public async Task ObterPorId_Nao_OK_Quando_Base_Nao_Disponivel()
        {
            var activity = new ActivitySource("ObterPorId_Nao_OK_Quando_Base_Nao_Disponivel");
            _mockFuncionarioRepositorioLeitura.Setup(x => x.ObterPorId(It.IsAny<Guid>()))
                .Throws(new Exception("Teste"));

            var service = new FuncionarioService(_mockFuncionarioRepositorioLeitura.Object, _mockFuncionarioRepositorioEscrita.Object,
                                                _mockNotificationService.Object, _mockLogger.Object, activity);

            await Assert.ThrowsAsync<Exception>(async () => await service.ObterPorId(Guid.NewGuid()));

            _mockFuncionarioRepositorioLeitura.Verify(x => x.ObterPorId(It.IsAny<Guid>()), Times.Once);
        }
        #endregion

        #region ObterTodos

        [Fact]
        public async Task ObterTodos_OK_Quando_Registro_Existe()
        {
            var activity = new ActivitySource("ObterTodos_OK_Quando_Registro_Existe");
            var person = _faker.Person;
            var id = Guid.NewGuid();
            var tels = new List<Telefone> {
                new Telefone("+55","11","90000-0000", id)
            };
            var endereco = new Endereco("Rua", 10, "00000-000", "apto", "bairro", "cidade", "sp", TipoEndereco.Residencial, id);
            var funcionario = new Funcionario(id.ToString(), "matricular", "cargo",
                new Nome(person.FirstName, person.LastName),
                new DataNascimento(new System.DateTime(1987, 08, 14)),
                new Email(person.Email), tels, endereco, endereco);

            _mockFuncionarioRepositorioLeitura.Setup(x => x.ObterTodos(It.IsAny<int>(), It.IsAny<int>()))
                .ReturnsAsync((new List<Funcionario>() { funcionario }, 1));

            var service = new FuncionarioService(_mockFuncionarioRepositorioLeitura.Object, _mockFuncionarioRepositorioEscrita.Object,
                                                _mockNotificationService.Object, _mockLogger.Object, activity);

            var result = await service.ObterTodos(1, 10);

            Output.WriteLine($"Result: {result}");

            _mockFuncionarioRepositorioLeitura.Verify(x => x.ObterTodos(It.IsAny<int>(), It.IsAny<int>()), Times.Once);
            result.Should().NotBeNull();
            result.Item1.Should().HaveCount(1);
        }

        [Fact]
        public async Task ObterTodos_OK_Quando_Registros_Nao_Existem()
        {
            var activity = new ActivitySource("ObterTodos_OK_Quando_Registros_Nao_Existem");
            _mockFuncionarioRepositorioLeitura.Setup(x => x.ObterTodos(It.IsAny<int>(), It.IsAny<int>()))
                .ReturnsAsync((new List<Funcionario>(), 0));

            var service = new FuncionarioService(_mockFuncionarioRepositorioLeitura.Object, _mockFuncionarioRepositorioEscrita.Object,
                                                    _mockNotificationService.Object, _mockLogger.Object, activity);

            var result = await service.ObterTodos(1, 10);

            Output.WriteLine($"Result: {result}");

            _mockFuncionarioRepositorioLeitura.Verify(x => x.ObterTodos(It.IsAny<int>(), It.IsAny<int>()), Times.Once);
            result.Should().NotBeNull();
            result.Item1.Should().HaveCount(0);
        }

        [Fact]
        public async Task ObterTodos_Nao_OK_Quando_Base_Nao_Disponivel()
        {
            var activity = new ActivitySource("ObterTodos_Nao_OK_Quando_Base_Nao_Disponivel");
            _mockFuncionarioRepositorioLeitura.Setup(x => x.ObterTodos(It.IsAny<int>(), It.IsAny<int>()))
                .Throws(new Exception("Teste"));

            var service = new FuncionarioService(_mockFuncionarioRepositorioLeitura.Object, _mockFuncionarioRepositorioEscrita.Object,
                                                _mockNotificationService.Object, _mockLogger.Object, activity);

            await Assert.ThrowsAsync<Exception>(async () => await service.ObterTodos(1, 10));

            _mockFuncionarioRepositorioLeitura.Verify(x => x.ObterTodos(It.IsAny<int>(), It.IsAny<int>()), Times.Once);
        }
        #endregion
    }
}