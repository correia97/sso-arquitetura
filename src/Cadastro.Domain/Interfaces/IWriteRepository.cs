using Domain.Entities;
using System;
using System.Threading.Tasks;

namespace Cadastro.Domain.Interfaces
{
    public interface IWriteRepository<T, U> : IDisposable where T : EntityBase<U>
    {
        void IniciarTransacao();
        void CompletarTransacao();
        void CancelarTransacao();
        Task<bool> Inserir(T data);
        Task<bool> Atualizar(T data);
    }
}
