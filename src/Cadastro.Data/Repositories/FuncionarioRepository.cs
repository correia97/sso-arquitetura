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
        public FuncionarioRepository(IDbConnection connection, ILogger<FuncionarioRepository> logger)
            : base(connection)
        {
            _logger = logger;
        }
        public async Task<Funcionario> ObterPorEmail(IDbTransaction transacao, string email)
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

            var result = await connection.QueryAsync<Funcionario, Nome, DataNascimento, Email, Funcionario>(query,
                    (funcionario, nome, dataNascimento, emailAddr) =>
                    {

                        funcionario.Atualizar(nome, dataNascimento, emailAddr, funcionario.Matricula, funcionario.Cargo);

                        return funcionario;
                    }, splitOn: "primeironome, date, enderecoemail", param: new { email }, transaction: transacao);
            return result.FirstOrDefault();

        }

        public override async Task<Funcionario> ObterPorId(IDbTransaction transacao, Guid id)
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


            var result = await connection.QueryAsync<Funcionario, Nome, DataNascimento, Email, Funcionario>(query,
                    (funcionario, nome, dataNascimento, emailAddr) =>
                    {

                        funcionario.Atualizar(nome, dataNascimento, emailAddr, funcionario.Matricula, funcionario.Cargo);

                        return funcionario;
                    }, splitOn: "primeironome, date, enderecoemail", param: paramId, transaction: transacao);
            return result.FirstOrDefault();

        }

        public override async Task<IEnumerable<Funcionario>> ObterTodos(IDbTransaction transacao)
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

            var result = await connection.QueryAsync<Funcionario, Nome, DataNascimento, Email, Funcionario>(query,
                    (funcionario, nome, dataNascimento, emailAddr) =>
                    {
                        funcionario.Atualizar(nome, dataNascimento, emailAddr, funcionario.Matricula, funcionario.Cargo);

                        return funcionario;
                    }, splitOn: "primeironome, date, enderecoemail", transaction: transacao);
            return result;
        }

        public async Task<List<Telefone>> ObterTelefonesPorFuncionarioId(IDbTransaction transacao, Guid funcionarioId)
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

            var result = await connection.QueryAsync<Telefone>(query, param, transaction: transacao);
            return result.ToList();

        }

        public async Task<List<Endereco>> ObterEnderecosPorFuncionarioId(IDbTransaction transacao, Guid funcionarioId)
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

            var result = await connection.QueryAsync<Endereco>(query, param, transaction: transacao);
            return result.ToList();

        }

        public override async Task<bool> Atualizar(Funcionario data, IDbTransaction transacao)
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
            param.Add("@dataCadastro", data.DataCadastro.ToUniversalTime(), DbType.DateTimeOffset);
            param.Add("@dataAtualizacao", DateTime.Now.ToUniversalTime(), DbType.DateTimeOffset);
            param.Add("@primeiroNome", data.Nome.PrimeiroNome);
            param.Add("@sobreNome", data.Nome.SobreNome);
            param.Add("@enderecoEmail", data.Email.EnderecoEmail);
            param.Add("@date", data.DataNascimento.Date > DateTime.MinValue ? data.DataNascimento.Date.ToUniversalTime() : null, DbType.DateTimeOffset);

            var result = await connection.ExecuteAsync(sql: query, param: param, transaction: transacao);
            return result > 0;
        }

        public override async Task<Guid> Inserir(Funcionario data, IDbTransaction transacao)
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
            param.Add("@dataCadastro", data.DataCadastro.ToUniversalTime(), DbType.DateTimeOffset);
            param.Add("@dataAtualizacao", data.DataAtualizacao.HasValue ? data.DataAtualizacao.Value.ToUniversalTime() : null, DbType.DateTimeOffset);
            param.Add("@primeiroNome", data.Nome.PrimeiroNome);
            param.Add("@sobreNome", data.Nome.SobreNome);
            param.Add("@enderecoEmail", data.Email.EnderecoEmail);
            param.Add("@date", data.DataNascimento.Date > DateTime.MinValue ? data.DataNascimento.Date.ToUniversalTime() : null, DbType.DateTimeOffset);


            var result = await connection.ExecuteAsync(sql: query, param: param, transaction: transacao);
            if (result > 0)
                return data.Id;

            return Guid.Empty;

        }

        public async Task<bool> AtualizarEndereco(Endereco endereco, IDbTransaction transacao)
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
            param.Add("@complemento", endereco.Complemento.Substring(0, 19));
            param.Add("@cep", endereco.CEP);
            param.Add("@uf", endereco.UF);
            param.Add("@cidade", endereco.Cidade);
            param.Add("@bairro", endereco.Bairro);
            param.Add("@tipoEndereco", endereco.TipoEndereco, DbType.Int32);
            param.Add("@id", endereco.Id);


            var result = await connection.ExecuteAsync(query, param, transacao);
            return result > 0;

        }

        public async Task<bool> AtualizarTelefone(Telefone telefone, IDbTransaction transacao)
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


            var result = await connection.ExecuteAsync(query, param, transacao);
            return result > 0;

        }

        public async Task<bool> InserirEndereco(Endereco endereco, IDbTransaction transacao)
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


            var result = await connection.ExecuteAsync(query, param, transacao);
            return result > 0;

        }

        public async Task<bool> InserirTelefone(Telefone telefone, IDbTransaction transacao)
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


            var result = await connection.ExecuteAsync(query, param, transacao);
            return result > 0;

        }

        public async Task<bool> RemoverEndereco(int id, IDbTransaction transacao)
        {
            var query = @"delete from public.enderecos
                           where id = @id";

            var param = new DynamicParameters();
            param.Add("@id", id);


            var result = await connection.ExecuteAsync(query, param, transacao);
            return result > 0;

        }

        public async Task<bool> RemoverTelefone(int id, IDbTransaction transacao)
        {
            var query = @"delete from public.telefones
                           where id = @id";

            var param = new DynamicParameters();
            param.Add("@id", id);

            var result = await connection.ExecuteAsync(query, param, transacao);
            return result > 0;

        }

    }
}
