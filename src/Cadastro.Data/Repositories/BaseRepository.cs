using Cadastro.Domain.Interfaces;
using Domain.Entities;
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
        protected readonly IDbConnection connection;
        protected BaseRepository(IDbConnection connection)
        {
            this.connection = connection;
        }
        public virtual IDbConnection RecuperarConexao()
        {
            return connection;
        }
        public abstract Task<IEnumerable<T>> ObterTodos(IDbTransaction transaction);

        public abstract Task<T> ObterPorId(IDbTransaction transaction, U id);

        public abstract Task<U> Inserir(T data, IDbTransaction transaction);

        public abstract Task<bool> Atualizar(T data, IDbTransaction transaction);

        public virtual IDbTransaction IniciarTransacao()
        {
            return connection.BeginTransaction();
        }

        public virtual void CancelarTransacao(IDbTransaction transaction)
        {
            transaction.Rollback();
        }

        public virtual void CompletarTransacao(IDbTransaction transaction)
        {
            transaction.Commit();
        }

    }
}
