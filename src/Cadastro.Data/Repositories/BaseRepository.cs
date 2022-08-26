using Cadastro.Domain.Interfaces;
using Domain.Entities;
using Microsoft.Extensions.Configuration;
using Npgsql;
using System.Collections.Generic;
using System.Data;
using System.Threading.Tasks;

namespace Cadastro.Data.Repositories
{
    public abstract class BaseRepository<T, U> :
        IWriteRepository<T, U>,
        IReadRepository<T, U> where T : EntityBase<U>
    {

        protected readonly string conexao;
        protected readonly IDbConnection dbConnection;
        protected BaseRepository(IConfiguration configuration)
        {
            conexao = configuration.GetConnectionString("Base");
            dbConnection = new NpgsqlConnection(conexao);
        }
        public virtual IDbConnection RecuperarConexao()
        {
            return dbConnection;
        }
        public abstract Task<IEnumerable<T>> ObterTodos(IDbTransaction transaction);

        public abstract Task<T> ObterPorId(IDbTransaction transaction, U id);

        public abstract Task<U> Inserir(T data, IDbTransaction transaction);

        public abstract Task<bool> Atualizar(T data, IDbTransaction transaction);

        public virtual async Task<IDbTransaction> IniciarTransacao()
        {
            AbrirConexao();
            return await ((NpgsqlConnection)dbConnection).BeginTransactionAsync();
        }

        public virtual async Task DesalocarConexao()
        {
            FecharConexao();
            await ((NpgsqlConnection)dbConnection).DisposeAsync();
        }

        public virtual async Task CancelarTransacao(IDbTransaction transaction)
        {
            await ((NpgsqlTransaction)transaction).RollbackAsync();
            FecharConexao();
        }

        public virtual async Task CompletarTransacao(IDbTransaction transaction)
        {
            await ((NpgsqlTransaction)transaction).CommitAsync();
            FecharConexao();
        }
        protected void AbrirConexao()
        {
            if (dbConnection.State != ConnectionState.Open)
                dbConnection.Open();
        }

        protected void FecharConexao()
        {
            if (dbConnection.State != ConnectionState.Open)
                dbConnection.Open();
        }
    }
}
