using Domain.Entities;
using System;
using System.Collections.Generic;
using System.Linq.Expressions;
using System.Threading.Tasks;

namespace API.Interfaces
{
    public interface IRepository<T, U> where T : EntityBase<U>
    {
        Task<T> GetById(U id);
        Task<IEnumerable<T>> GetAll();
        Task<U> Insert(T data);
        Task<bool> Update(T data);

    }
}
