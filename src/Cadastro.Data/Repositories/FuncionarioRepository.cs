using Cadastro.Domain.Interfaces;
using Dapper;
using Domain.Entities;
using Domain.ValueObject;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Threading.Tasks;

namespace Cadastro.Data.Repositories
{
    public class FuncionarioRepository : BaseRepository<Funcionario, Guid>, IFuncionarioReadRepository, IFuncionarioWriteRepository
    {
        private readonly ILogger<FuncionarioRepository> _logger;
        public FuncionarioRepository(IConfiguration configuration, ILogger<FuncionarioRepository> logger)
            : base(configuration)
        {
            _logger = logger;
        }

        public override async Task<bool> Atualizar(Funcionario data)
        {
            var query = @"UPDATE public.funcionarios SET
                          matricula       =@matricula
                        , cargo           =@cargo
                        , ativo           =@ativo
                        , datacadastro    =@dataCadastro
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
            param.Add("@dataCadastro", DateTimeOffset.FromFileTime(data.DataCadastro.ToFileTimeUtc()), DbType.DateTimeOffset);
            param.Add("@dataAtualizacao", DateTimeOffset.FromFileTime(DateTime.Now.ToFileTimeUtc()), DbType.DateTimeOffset);
            param.Add("@primeiroNome", data.Nome.PrimeiroNome);
            param.Add("@sobreNome", data.Nome.SobreNome);
            param.Add("@enderecoEmail", data.Email.EnderecoEmail);
            param.Add("@date", data.DataNascimento.Date > DateTime.MinValue ? DateTimeOffset.FromFileTime(data.DataNascimento.Date.ToFileTimeUtc()) : null, DbType.DateTimeOffset);

            try
            {
                var result = await connection.ExecuteAsync(query, param);
                return result > 0;
            }
            catch (Npgsql.NpgsqlOperationInProgressException ex)
            {
                _logger.LogError(ex, "Atualizar erro");
                throw;
            }

            catch (Npgsql.PostgresException ex)
            {
                _logger.LogError(ex, "Atualizar erro");
                throw;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Atualizar erro");
                throw;
            }
        }

        public override async Task<Guid> Inserir(Funcionario data)
        {
            var query = @"INSERT into public.funcionarios
                           (id
                        , userid
                        , matricula
                        , cargo
                        , ativo
                        , datacadastro
                        , dataatualizacao
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
                        ,@dataAtualizacao
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
            param.Add("@dataCadastro", DateTimeOffset.FromFileTime(data.DataCadastro.ToFileTimeUtc()), DbType.DateTimeOffset);
            param.Add("@dataAtualizacao", data.DataAtualizacao.HasValue ? DateTimeOffset.FromFileTime(data.DataAtualizacao.Value.ToFileTimeUtc()) : null, DbType.DateTimeOffset);
            param.Add("@primeiroNome", data.Nome.PrimeiroNome);
            param.Add("@sobreNome", data.Nome.SobreNome);
            param.Add("@enderecoEmail", data.Email.EnderecoEmail);
            param.Add("@date", data.DataNascimento.Date > DateTime.MinValue ? DateTimeOffset.FromFileTime(data.DataNascimento.Date.ToFileTimeUtc()) : null, DbType.DateTimeOffset);

            try
            {
                var result = await connection.ExecuteAsync(query, param);
                return data.Id;
            }
            catch (Npgsql.NpgsqlOperationInProgressException ex)
            {
                _logger.LogError(ex, "Inserir erro");
                throw;
            }

            catch (Npgsql.PostgresException ex)
            {
                _logger.LogError(ex, "Inserir erro");
                throw;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Inserir erro");
                throw;
            }
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
                        from public.funcionarios
                        where enderecoemail = @email";
            try
            {
                var result = await connection.QueryAsync<Funcionario, Nome, DataNascimento, Email, Funcionario>(query,
                    (funcionario, nome, dataNascimento, emailAddr) =>
                    {

                        funcionario.Atualizar(nome, dataNascimento, emailAddr, funcionario.Matricula, funcionario.Cargo);

                        return funcionario;
                    }, splitOn: "primeironome, date, enderecoemail", param: new { email });
                return result.FirstOrDefault();
            }
            catch (Npgsql.NpgsqlOperationInProgressException ex)
            {
                _logger.LogError(ex, "ObterPorEmail erro");
                throw;
            }

            catch (Npgsql.PostgresException ex)
            {
                _logger.LogError(ex, "ObterPorEmail erro");
                throw;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "ObterPorEmail erro");
                throw;
            }
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

            try
            {
                var result = await connection.QueryAsync<Funcionario, Nome, DataNascimento, Email, Funcionario>(query,
                    (funcionario, nome, dataNascimento, emailAddr) =>
                    {

                        funcionario.Atualizar(nome, dataNascimento, emailAddr, funcionario.Matricula, funcionario.Cargo);

                        return funcionario;
                    }, splitOn: "primeironome, date, enderecoemail", param: paramId);
                return result.FirstOrDefault();
            }
            catch (Npgsql.NpgsqlOperationInProgressException ex)
            {
                _logger.LogError(ex, "ObterPorId erro");
                throw;
            }
            catch (Npgsql.PostgresException ex)
            {
                _logger.LogError(ex, "ObterPorId erro");
                throw;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "ObterPorId erro");
                throw;
            }
        }

        public override async Task<IEnumerable<Funcionario>> ObterTodos()
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
                        FROM public.funcionarios";
            try
            {
                var result = await connection.QueryAsync<Funcionario, Nome, DataNascimento, Email, Funcionario>(query,
                    (funcionario, nome, dataNascimento, emailAddr) =>
                    {
                        funcionario.Atualizar(nome, dataNascimento, emailAddr, funcionario.Matricula, funcionario.Cargo);

                        return funcionario;
                    }, splitOn: "primeironome, date, enderecoemail");
                return result;
            }
            catch (Npgsql.NpgsqlOperationInProgressException ex)
            {
                _logger.LogError(ex, "ObterTodos erro");
                throw;
            }
            catch (Npgsql.PostgresException ex)
            {
                _logger.LogError(ex, "ObterTodos erro");
                throw;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "ObterTodos erro");
                throw;
            }
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
            param.Add("@complemento", endereco.Complemento);
            param.Add("@cep", endereco.CEP);
            param.Add("@uf", endereco.UF);
            param.Add("@cidade", endereco.Cidade);
            param.Add("@bairro", endereco.Bairro);
            param.Add("@tipoEndereco", endereco.TipoEndereco, DbType.Int32);
            param.Add("@id", endereco.Id);

            try
            {
                var result = await connection.ExecuteAsync(query, param);
                return result > 0;
            }
            catch (Npgsql.NpgsqlOperationInProgressException ex)
            {
                _logger.LogError(ex, "AtualizarEndereco erro");
                throw;
            }

            catch (Npgsql.PostgresException ex)
            {
                _logger.LogError(ex, "AtualizarEndereco erro");
                throw;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "AtualizarEndereco erro");
                throw;
            }
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

            try
            {
                var result = await connection.ExecuteAsync(query, param);
                return result > 0;
            }
            catch (Npgsql.NpgsqlOperationInProgressException ex)
            {
                _logger.LogError(ex, "AtualizarTelefone erro");
                throw;
            }

            catch (Npgsql.PostgresException ex)
            {
                _logger.LogError(ex, "AtualizarTelefone erro");
                throw;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "AtualizarTelefone erro");
                throw;
            }
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

            try
            {
                var result = await connection.ExecuteAsync(query, param);
                return result > 0;
            }
            catch (Npgsql.NpgsqlOperationInProgressException ex)
            {
                _logger.LogError(ex, "InserirEndereco erro");
                throw;
            }

            catch (Npgsql.PostgresException ex)
            {
                _logger.LogError(ex, "InserirEndereco erro");
                throw;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "InserirEndereco erro");
                throw;
            }
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

            try
            {
                var result = await connection.ExecuteAsync(query, param);
                return result > 0;
            }
            catch (Npgsql.NpgsqlOperationInProgressException ex)
            {
                _logger.LogError(ex, "InserirTelefone erro");
                throw;
            }

            catch (Npgsql.PostgresException ex)
            {
                _logger.LogError(ex, "InserirTelefone erro");
                throw;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "InserirTelefone erro");
                throw;
            }
        }

        public async Task<bool> RemoverEndereco(int id)
        {
            var query = @"delete from public.enderecos
                           where id = @id";

            var param = new DynamicParameters();
            param.Add("@id", id);

            try
            {
                var result = await connection.ExecuteAsync(query, param);
                return result > 0;
            }
            catch (Npgsql.NpgsqlOperationInProgressException ex)
            {
                _logger.LogError(ex, "RemoverEndereco erro");
                throw;
            }

            catch (Npgsql.PostgresException ex)
            {
                _logger.LogError(ex, "RemoverEndereco erro");
                throw;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "RemoverEndereco erro");
                throw;
            }
        }

        public async Task<bool> RemoverTelefone(int id)
        {
            var query = @"delete from public.telefones
                           where id = @id";

            var param = new DynamicParameters();
            param.Add("@id", id);

            try
            {
                var result = await connection.ExecuteAsync(query, param);
                return result > 0;
            }
            catch (Npgsql.NpgsqlOperationInProgressException ex)
            {
                _logger.LogError(ex, "RemoverTelefone erro");
                throw;
            }

            catch (Npgsql.PostgresException ex)
            {
                _logger.LogError(ex, "RemoverTelefone erro");
                throw;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "RemoverTelefone erro");
                throw;
            }
        }

        public async Task<List<Telefone>> ObterTelefonesPorFuncionarioId(Guid funcionarioId)
        {
            var query = @"Select id
                        , ddi
                        , ddd
                        , numeroTelefone
                        , funcionarioId
                        from public.telefones
                         where funcionarioId = @funcionarioId";

            var param = new DynamicParameters();
            param.Add("@funcionarioId", funcionarioId);

            try
            {
                var result = await connection.QueryAsync<Telefone>(query, param);
                return result.ToList();
            }
            catch (Npgsql.NpgsqlOperationInProgressException ex)
            {
                _logger.LogError(ex, "ObterTelefonesPorFuncionarioId erro");
                throw;
            }

            catch (Npgsql.PostgresException ex)
            {
                _logger.LogError(ex, "ObterTelefonesPorFuncionarioId erro");
                throw;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "ObterTelefonesPorFuncionarioId erro");
                throw;
            }
        }

        public async Task<List<Endereco>> ObterEnderecosPorFuncionarioId(Guid funcionarioId)
        {
            var query = @"Select id
                        , rua        
                        , numero     
                        , complemento
                        , cep        
                        , uf         
                        , cidade     
                        , bairro     
                        , tipoEndereco     
                        , funcionarioId 
                        from public.enderecos
                         where funcionarioId = @funcionarioId";

            var param = new DynamicParameters();
            param.Add("@funcionarioId", funcionarioId);

            try
            {
                var result = await connection.QueryAsync<Endereco>(query, param);
                return result.ToList();
            }
            catch (Npgsql.NpgsqlOperationInProgressException ex)
            {
                _logger.LogError(ex, "ObterEnderecosPorFuncionarioId erro");
                throw;
            }

            catch (Npgsql.PostgresException ex)
            {
                _logger.LogError(ex, "ObterEnderecosPorFuncionarioId erro");
                throw;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "ObterEnderecosPorFuncionarioId erro");
                throw;
            }
        }
    }
}
