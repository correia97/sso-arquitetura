using Cadastro.Domain.Interfaces;

using Domain.Entities;
using Microsoft.Extensions.Configuration;
using Npgsql;
using System.Collections.Generic;
using System.Data;
using System.Data.Common;
using System.Threading.Tasks;
using System.Transactions;

namespace Cadastro.Data.Repositories
{
    public abstract class BaseRepository<T, U> :
        IWriteRepository<T, U>,
        IReadRepository<T, U> where T : EntityBase<U>
    {

        protected readonly string conexao;
        protected BaseRepository(IConfiguration configuration)
        {
            conexao = configuration.GetConnectionString("Base");
        }
        public virtual IDbConnection RecuperarConexao()
        {
            return new NpgsqlConnection(conexao);
        }
        public abstract Task<IEnumerable<T>> ObterTodos(IDbConnection dbConnection, IDbTransaction transaction);

        public abstract Task<T> ObterPorId(IDbConnection dbConnection, IDbTransaction transaction, U id);

        public abstract Task<U> Inserir(T data, IDbConnection dbConnection, IDbTransaction transaction);

        public abstract Task<bool> Atualizar(T data, IDbConnection dbConnection, IDbTransaction transaction);

        public virtual async Task<IDbTransaction> IniciarTransacao(IDbConnection dbConnection)
        {
            AbrirConexao(dbConnection);
            return await ((NpgsqlConnection)dbConnection).BeginTransactionAsync();
        }


        public virtual async Task DesalocarConexao(IDbConnection dbConnection)
        {
            FecharConexao(dbConnection);
            await ((NpgsqlConnection)dbConnection).DisposeAsync();
        }

        public virtual async Task CancelarTransacao(IDbConnection dbConnection, IDbTransaction transaction)
        {
            await ((NpgsqlTransaction)transaction).RollbackAsync();
            FecharConexao(dbConnection);
        }

        public virtual async Task CompletarTransacao(IDbConnection dbConnection, IDbTransaction transaction)
        {
            await ((NpgsqlTransaction)transaction).CommitAsync();
            FecharConexao(dbConnection);
        }
        protected void AbrirConexao(IDbConnection dbConnection)
        {
            if (dbConnection.State != ConnectionState.Open)
                dbConnection.Open();
        }

        protected void FecharConexao(IDbConnection dbConnection)
        {
            if (dbConnection.State != ConnectionState.Open)
                dbConnection.Open();
        }
    }
}
