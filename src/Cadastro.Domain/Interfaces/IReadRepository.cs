using Domain.Entities;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Cadastro.Domain.Interfaces
{
    public interface IReadRepository<T, U> where T : EntityBase<U>
    {
        Task<T> ObterPorId(U id);
        Task<IEnumerable<T>> ObterTodos();

    }
}
