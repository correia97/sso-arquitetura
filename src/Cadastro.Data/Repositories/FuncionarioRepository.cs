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
            var query = @"UPDATE funcionarios SET
                          userid          =@userId
                        , matricula       =@matricula
                        , cargo           =@cargo
                        , ativo           =@ativo
                        , datacadastro    =@dataCadastro
                        , dataatualizacao =@dataAtualizacao
                        , primeironome    =@primeiroNome
                        , sobrenome       =@sobreNome
                        , enderecoemail   =@enderecoEmail  
                        , datanascimento  =@date
                        WHERE id          =@id";

            var param = new DynamicParameters();
            param.Add("@id", data.Id);
            param.Add("@userId", data.UserId);
            param.Add("@matricula", data.Matricula);
            param.Add("@cargo", data.Cargo);
            param.Add("@ativo", data.Ativo, DbType.Boolean);
            param.Add("@dataCadastro", data.DataCadastro, DbType.DateTimeOffset);
            param.Add("@dataAtualizacao", data.DataAtualizacao, DbType.DateTimeOffset);
            param.Add("@primeiroNome", data.Nome.PrimeiroNome);
            param.Add("@sobreNome", data.Nome.SobreNome);
            param.Add("@enderecoEmail", data.Email.EnderecoEmail);
            param.Add("@date", data.DataNascimento, DbType.DateTimeOffset);

            try
            {
                var result = await connection.ExecuteAsync(query, param);
                return result > 0;
            }
            catch (Exception ex)
            {
                _logger.LogError("Atualizar erro", ex);
                throw;
            }
        }


        public override async Task<Guid> Inserir(Funcionario data)
        {
            var query = @"INSERT into funcionarios
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
            param.Add("@matricula", data.Matricula);
            param.Add("@cargo", data.Cargo);
            param.Add("@ativo", data.Ativo, DbType.Boolean);
            param.Add("@dataCadastro", DateTimeOffset.FromFileTime(data.DataCadastro.ToFileTimeUtc()), DbType.DateTimeOffset);
            param.Add("@dataAtualizacao", data.DataAtualizacao.HasValue ? DateTimeOffset.FromFileTime(data.DataAtualizacao.Value.ToFileTimeUtc()) : null, DbType.DateTimeOffset);
            param.Add("@primeiroNome", data.Nome.PrimeiroNome);
            param.Add("@sobreNome", data.Nome.SobreNome);
            param.Add("@enderecoEmail", data.Email.EnderecoEmail);
            param.Add("@date", DateTimeOffset.FromFileTime(data.DataNascimento.Date.ToFileTimeUtc()), DbType.DateTimeOffset);

            try
            {
                var result = await connection.ExecuteAsync(query, param);
                return data.Id;
            }
            catch (Exception ex)
            {
                _logger.LogError("Inserir erro", ex);
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
                        from funcionarios
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
            catch (Exception ex)
            {
                _logger.LogError("ObterPorEmail erro", ex);
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
                        FROM funcionarios
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
            catch (Exception ex)
            {
                _logger.LogError("ObterPorId erro", ex);
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
                        FROM funcionarios";
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
            catch (Exception ex)
            {
                _logger.LogError("ObterTodos erro", ex);
                throw;
            }
        }
    }
}
