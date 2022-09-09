using Cadastro.Domain.Enums;
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

        public IDbTransaction Transaction { get; private set; }
        private TransactionStatusEnum Status { get; set; } = TransactionStatusEnum.None;
        protected BaseRepository(IDbConnection connection)
        {
            this.connection = connection;
        }

        public virtual void IniciarTransacao()
        {
            Transaction = connection.BeginTransaction();
            Status = TransactionStatusEnum.Started;
        }

        public virtual void CompletarTransacao()
        {
            Transaction.Commit();
            Status = TransactionStatusEnum.Completed;
        }

        public virtual void CancelarTransacao()
        {
            Transaction.Rollback();
            Status = TransactionStatusEnum.Canceled;
        }
        public virtual void Dispose()
        {
            if (Status == TransactionStatusEnum.Started)
                CancelarTransacao();
        }

        public abstract Task<IEnumerable<T>> ObterTodos();

        public abstract Task<T> ObterPorId( U id);

        public abstract Task<U> Inserir(T data);

        public abstract Task<bool> Atualizar(T data);      

    }
}
