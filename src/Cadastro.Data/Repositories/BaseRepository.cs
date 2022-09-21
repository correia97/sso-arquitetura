using Cadastro.Domain.Enums;
using Cadastro.Domain.Interfaces;
using Domain.Entities;
using System;
using System.Collections.Generic;
using System.Data;
using System.Threading.Tasks;

namespace Cadastro.Data.Repositories
{
    public abstract class BaseRepository<T, U> :
        IWriteRepository<T, U>,
        IReadRepository<T, U> where T : EntityBase<U>
    {
        protected IDbConnection Connection { get; set; }
        protected IDbTransaction Transaction { get; set; }
        protected TransactionStatusEnum Status { get; set; }
        protected BaseRepository(IDbConnection connection)
        {
            this.Connection = connection;
        }

        public abstract Task<IEnumerable<T>> ObterTodos();

        public abstract Task<T> ObterPorId(U id);

        public abstract Task<bool> Inserir(T data);

        public abstract Task<bool> Atualizar(T data);

        public virtual void IniciarTransacao()
        {
            Transaction = Connection.BeginTransaction();
            Status = TransactionStatusEnum.Started;
        }

        public virtual void CompletarTransacao()
        {
            Transaction?.Commit();
            Status = TransactionStatusEnum.Completed;
        }

        public virtual void CancelarTransacao()
        {
            Transaction?.Rollback();
            Status = TransactionStatusEnum.Canceled;
        }
        public virtual void Dispose()
        {
            if (Status == TransactionStatusEnum.Started)
                Transaction?.Rollback();
            Status = TransactionStatusEnum.None;
            Transaction?.Dispose();

            GC.SuppressFinalize(this);
        }

    }
}
