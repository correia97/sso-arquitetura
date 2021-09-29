using Domain.Entities;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Cadastro.Domain.Interfaces
{
    public interface IWriteRepository<T, U> where T : EntityBase<U>
    {
        Task<U> Inserir(T data);
        Task<bool> Atualizar(T data);
    }
}
