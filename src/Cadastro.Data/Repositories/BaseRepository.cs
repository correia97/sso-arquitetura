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
        protected TransactionStatus Status { get; set; }
        protected BaseRepository(IDbConnection connection)
        {
            this.Connection = connection;
        }

        public abstract Task<(IEnumerable<Funcionario>, int)> ObterTodos(int pagina, int qtdItens);

        public abstract Task<T> ObterPorId(U id);

        public abstract Task<bool> Inserir(T data);

        public abstract Task<bool> Atualizar(T data);

        public abstract Task<bool> Remover(U id);

        public virtual void IniciarTransacao()
        {
            Transaction = Connection.BeginTransaction();
            Status = TransactionStatus.Started;
        }

        public virtual void CompletarTransacao()
        {
            Transaction?.Commit();
            Status = TransactionStatus.Completed;
        }

        public virtual void CancelarTransacao()
        {
            Transaction?.Rollback();
            Status = TransactionStatus.Canceled;
        }
        public void Dispose()
        {
            if (Status == TransactionStatus.Started)
                Transaction?.Rollback();
            Status = TransactionStatus.None;
            Transaction?.Dispose();
            GC.SuppressFinalize(this);
        }
    }
}
