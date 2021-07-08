using Domain.Entities;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Cadastro.Domain.Interfaces
{
    public interface IRepository<T, U> where T : EntityBase<U>
    {
        Task<T> RecuperarPorId(U id);
        Task<IEnumerable<T>> RecuperarTodos();
        Task<U> Inserir(T data);
        Task<bool> Atualizar(T data);
    }
}
