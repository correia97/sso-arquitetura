using Cadastro.Domain.Enums;
using Cadastro.Domain.Interfaces;
using Dapper;
using Domain.Entities;
using Domain.ValueObject;
using Microsoft.Extensions.Logging;
using Npgsql;
using Polly;
using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace Cadastro.Data.Repositories
{
    public class FuncionarioRepository : BaseRepository<Funcionario, Guid>, IFuncionarioReadRepository, IFuncionarioWriteRepository
    {
        private readonly ILogger<FuncionarioRepository> _logger;
        private readonly AsyncPolicy _retryPolicy;
        public FuncionarioRepository(IDbConnection connection, ILogger<FuncionarioRepository> logger)
            : base(connection)
        {
            _logger = logger;
            _retryPolicy = Policy.Handle<PostgresException>()
                        .Or<Exception>()
                        .WaitAndRetryAsync(3, retryAttempt => TimeSpan.FromSeconds(Math.Pow(3, retryAttempt)),
                        (exception, timeSpan, retryCount, context) =>
                        {
                            _logger.LogError(exception, $"Retry {retryCount} at: {DateTime.Now:dd/MM/yyyy HH:mm:ss}");

                            if (exception.Message.Contains("A transaction is already in progress"))
                                Thread.Sleep(100);
                        });
            Status = TransactionStatus.None;
        }

        public async Task<Funcionario> ObterPorEmail(string email)
        {
            var query = @"SELECT  id
                        , userid
                        , matricula
                        , cargo
                        , ativo
                        , datacadastro
                        , dataatualizacao
                        , primeironome
                        , sobrenome
                        , datanascimento as date
                        , enderecoemail
                        FROM public.funcionarios
                        WHERE enderecoemail = @email";

            var result = await _retryPolicy.ExecuteAsync(() => Connection.QueryAsync<Funcionario, Nome, DataNascimento, Email, Funcionario>(query,
                    (funcionario, nome, dataNascimento, emailAddr) =>
                    {
                        funcionario.Atualizar(nome, dataNascimento, emailAddr, funcionario.Matricula, funcionario.Cargo);
                        return funcionario;
                    }, splitOn: "primeironome, date, enderecoemail", param: new { email }, transaction: Transaction));
            return result.FirstOrDefault();
        }

        public override async Task<Funcionario> ObterPorId(Guid id)
        {
            var query = @"SELECT   id
                        , userid
                        , matricula
                        , cargo
                        , ativo
                        , datacadastro
                        , dataatualizacao
                        , primeironome
                        , sobrenome
                        , datanascimento as date
                        , enderecoemail
                        FROM public.funcionarios
                        WHERE userid = @id";

            var paramId = new DynamicParameters();
            paramId.Add("@id", id.ToString(), DbType.String);

            var result = await _retryPolicy.ExecuteAsync(() => Connection.QueryAsync<Funcionario, Nome, DataNascimento, Email, Funcionario>(query,
                    (funcionario, nome, dataNascimento, emailAddr) =>
                    {
                        funcionario.Atualizar(nome, dataNascimento, emailAddr, funcionario.Matricula, funcionario.Cargo);
                        return funcionario;
                    }, splitOn: "primeironome, date, enderecoemail", param: paramId, transaction: Transaction));
            return result.FirstOrDefault();
        }

        public override async Task<(IEnumerable<Funcionario>, int)> ObterTodos(int pagina, int qtdItens)
        {
            var query = @"SELECT   id
                        , userid
                        , matricula
                        , cargo
                        , ativo
                        , datacadastro
                        , dataatualizacao
                        , primeironome
                        , sobrenome
                        , datanascimento as date
                        , enderecoemail
                        , (SELECT count(id) FROM public.funcionarios) as total
                        FROM public.funcionarios
                        ORDER BY datacadastro
                        LIMIT @qtd
                        OFFSET @pular";


            var @params = new DynamicParameters();
            @params.Add("@qtd", qtdItens, DbType.Int32);
            @params.Add("@pular", pagina * qtdItens, DbType.Int32);

            Int64 qtdTotal = 0;
            var result = await _retryPolicy.ExecuteAsync(() => Connection.QueryAsync<Funcionario, Nome, DataNascimento, Email, Int64, Funcionario>(query,
                    (funcionario, nome, dataNascimento, emailAddr, total) =>
                    {
                        qtdTotal = total;
                        funcionario.Atualizar(nome, dataNascimento, emailAddr, funcionario.Matricula, funcionario.Cargo);
                        return funcionario;
                    }, splitOn: "primeironome, date, enderecoemail, total", transaction: Transaction, param: @params));
            return (result, ((int)qtdTotal));
        }

        public async Task<List<Telefone>> ObterTelefonesPorFuncionarioId(Guid funcionarioId)
        {
            var query = @"SELECT id
                        , ddi
                        , ddd
                        , numeroTelefone
                        , funcionarioId
                         FROM public.telefones
                         WHERE funcionarioId = @funcionarioId";

            var param = new DynamicParameters();
            param.Add("@funcionarioId", funcionarioId);

            var result = await _retryPolicy.ExecuteAsync(() => Connection.QueryAsync<Telefone>(query, param, transaction: Transaction));
            return result.ToList();
        }

        public async Task<List<Endereco>> ObterEnderecosPorFuncionarioId(Guid funcionarioId)
        {
            var query = @"SELECT id
                        , rua        
                        , numero     
                        , complemento
                        , cep        
                        , uf         
                        , cidade     
                        , bairro     
                        , tipoEndereco     
                        , funcionarioId 
                         FROM public.enderecos
                         WHERE funcionarioId = @funcionarioId";

            var param = new DynamicParameters();
            param.Add("@funcionarioId", funcionarioId);

            var result = await _retryPolicy.ExecuteAsync(() => Connection.QueryAsync<Endereco>(query, param, transaction: Transaction));
            return result.ToList();
        }

        public override async Task<bool> Atualizar(Funcionario data)
        {
            var query = @"UPDATE public.funcionarios SET
                          matricula       =@matricula
                        , cargo           =@cargo
                        , ativo           =@ativo
                        , dataatualizacao =@dataAtualizacao
                        , primeironome    =@primeiroNome
                        , sobrenome       =@sobreNome
                        , enderecoemail   =@enderecoEmail  
                        , datanascimento  =@date
                        WHERE userId      =@userId";

            var param = new DynamicParameters();
            param.Add("@userId", data.UserId);
            param.Add("@matricula", string.IsNullOrEmpty(data.Matricula) ? "" : data.Matricula);
            param.Add("@cargo", string.IsNullOrEmpty(data.Cargo) ? "" : data.Cargo);
            param.Add("@ativo", data.Ativo, DbType.Boolean);
            param.Add("@dataAtualizacao", DateTime.Now.ToUniversalTime(), DbType.DateTime);
            param.Add("@primeiroNome", data.Nome.PrimeiroNome);
            param.Add("@sobreNome", data.Nome.SobreNome);
            param.Add("@enderecoEmail", data.Email.EnderecoEmail);
            param.Add("@date", data.DataNascimento.Date > DateTime.MinValue ? data.DataNascimento.Date.ToUniversalTime() : null, DbType.DateTime);

            var result = await _retryPolicy.ExecuteAsync(() => Connection.ExecuteAsync(sql: query, param: param, transaction: Transaction));
            return result > 0;
        }

        public override async Task<bool> Inserir(Funcionario data)
        {
            var query = @"INSERT into public.funcionarios
                           (id
                        , userid
                        , matricula
                        , cargo
                        , ativo
                        , datacadastro                        
                        , primeironome
                        , sobrenome
                        , enderecoemail
                        , datanascimento) VALUES (
                         @id
                        ,@userId
                        ,@matricula
                        ,@cargo
                        ,@ativo
                        ,@dataCadastro                        
                        ,@primeiroNome
                        ,@sobreNome
                        ,@enderecoEmail
                        ,@date )";

            var param = new DynamicParameters();
            param.Add("@id", data.Id);
            param.Add("@userId", data.UserId);
            param.Add("@matricula", string.IsNullOrEmpty(data.Matricula) ? "" : data.Matricula);
            param.Add("@cargo", string.IsNullOrEmpty(data.Cargo) ? "" : data.Cargo);
            param.Add("@ativo", data.Ativo, DbType.Boolean);
            param.Add("@dataCadastro", data.DataCadastro.ToUniversalTime(), DbType.DateTime);
            param.Add("@primeiroNome", data.Nome.PrimeiroNome);
            param.Add("@sobreNome", data.Nome.SobreNome);
            param.Add("@enderecoEmail", data.Email.EnderecoEmail);
            param.Add("@date", data.DataNascimento.Date > DateTime.MinValue ? data.DataNascimento.Date.ToUniversalTime() : null, DbType.DateTime);

            var result = await _retryPolicy.ExecuteAsync(() => Connection.ExecuteAsync(sql: query, param: param, transaction: Transaction));

            return result > 0;
        }

        public async Task<bool> AtualizarEndereco(Endereco endereco)
        {
            var query = @"UPDATE public.enderecos SET
                          rua           =@rua
                        , numero        =@numero
                        , complemento   =@complemento
                        , cep           =@cep
                        , uf            =@uf
                        , cidade        =@cidade
                        , bairro        =@bairro
                        , tipoEndereco  =@tipoEndereco
                        WHERE id        =@id";

            var param = new DynamicParameters();
            param.Add("@rua", endereco.Rua);
            param.Add("@numero", endereco.Numero);
            param.Add("@complemento", string.IsNullOrEmpty(endereco.Complemento) ? null : endereco.Complemento.Substring(0, 19));
            param.Add("@cep", endereco.CEP);
            param.Add("@uf", endereco.UF);
            param.Add("@cidade", endereco.Cidade);
            param.Add("@bairro", endereco.Bairro);
            param.Add("@tipoEndereco", endereco.TipoEndereco, DbType.Int32);
            param.Add("@id", endereco.Id);

            var result = await _retryPolicy.ExecuteAsync(() => Connection.ExecuteAsync(query, param, Transaction));
            return result > 0;
        }

        public async Task<bool> AtualizarTelefone(Telefone telefone)
        {
            var query = @"UPDATE public.telefones SET
                          ddi                       =@ddi
                        , ddd                       =@ddd
                        , numeroTelefone            =@numeroTelefone
                        WHERE id                    =@id";


            var param = new DynamicParameters();
            param.Add("@id", telefone.Id);
            param.Add("@ddi", telefone.DDI);
            param.Add("@ddd", telefone.DDD);
            param.Add("@numeroTelefone", telefone.NumeroTelefone);

            var result = await _retryPolicy.ExecuteAsync(() => Connection.ExecuteAsync(query, param, Transaction));
            return result > 0;
        }

        public async Task<bool> InserirEndereco(Endereco endereco)
        {
            var query = @"INSERT into public.enderecos
                        ( rua        
                        , numero     
                        , complemento
                        , cep        
                        , uf         
                        , cidade     
                        , bairro     
                        , tipoEndereco     
                        , funcionarioId) VALUES 
                        ( @rua
                        , @numero
                        , @complemento
                        , @cep
                        , @uf
                        , @cidade
                        , @bairro
                        , @tipoEndereco     
                        , @funcionarioId)";

            var param = new DynamicParameters();
            param.Add("@rua", endereco.Rua);
            param.Add("@numero", endereco.Numero);
            param.Add("@complemento", endereco.Complemento);
            param.Add("@cep", endereco.CEP);
            param.Add("@uf", endereco.UF);
            param.Add("@cidade", endereco.Cidade);
            param.Add("@bairro", endereco.Bairro);
            param.Add("@tipoEndereco", endereco.TipoEndereco, DbType.Int32);
            param.Add("@funcionarioId", endereco.FuncionarioId);

            var result = await _retryPolicy.ExecuteAsync(() => Connection.ExecuteAsync(query, param, Transaction));
            return result > 0;
        }

        public async Task<bool> InserirTelefone(Telefone telefone)
        {
            var query = @"INSERT into public.telefones
                           (ddi
                        , ddd
                        , numeroTelefone
                        , funcionarioId) VALUES (
                         @ddd
                        ,@ddi
                        ,@telefone
                        ,@funcionarioId)";

            var param = new DynamicParameters();
            param.Add("@ddi", telefone.DDI);
            param.Add("@ddd", telefone.DDD);
            param.Add("@telefone", telefone.NumeroTelefone);
            param.Add("@funcionarioId", telefone.FuncionarioId);

            var result = await _retryPolicy.ExecuteAsync(() => Connection.ExecuteAsync(query, param, Transaction));
            return result > 0;
        }

        public async Task<bool> RemoverEndereco(int id)
        {
            var query = @"DELETE from public.enderecos
                           WHERE id = @id";

            var param = new DynamicParameters();
            param.Add("@id", id);

            var result = await _retryPolicy.ExecuteAsync(() => Connection.ExecuteAsync(query, param, Transaction));
            return result > 0;
        }

        public async Task<bool> RemoverTelefone(int id)
        {
            var query = @"DELETE from public.telefones
                           WHERE id = @id";

            var param = new DynamicParameters();
            param.Add("@id", id);

            var result = await _retryPolicy.ExecuteAsync(() => Connection.ExecuteAsync(query, param, Transaction));
            return result > 0;
        }

        public async Task<bool> Desativar(Guid id)
        {
            var query = @"UPDATE public.funcionarios SET                       
                          ativo           =@ativo                        
                        , dataatualizacao =@dataAtualizacao                        
                        WHERE userId      =@userId";

            var param = new DynamicParameters();
            param.Add("@userId", id);
            param.Add("@dataAtualizacao", DateTime.Now.ToUniversalTime(), DbType.DateTime);

            var result = await _retryPolicy.ExecuteAsync(() => Connection.ExecuteAsync(sql: query, param: param, transaction: Transaction));
            return result > 0;
        }

        public override async Task<bool> Remover(Guid id)
        {
            var query = @"DELETE from public.funcionarios
                           WHERE userId = @id";

            var param = new DynamicParameters();
            param.Add("@id", id.ToString(), DbType.String);

            var result = await _retryPolicy.ExecuteAsync(() => Connection.ExecuteAsync(query, param, Transaction));
            return result > 0;
        }
    }
}
