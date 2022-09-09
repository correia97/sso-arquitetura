using Domain.Entities;
using System;
using System.Data;
using System.Threading.Tasks;

namespace Cadastro.Domain.Interfaces
{
    public interface IWriteRepository<T, U> : IDisposable where T : EntityBase<U>
    {
        IDbTransaction Transaction { get; }
        void IniciarTransacao();
        void CancelarTransacao();
        void CompletarTransacao();
        Task<U> Inserir(T data);
        Task<bool> Atualizar(T data); 
    }
}
