using Cadastro.Domain.Interfaces;
using Domain.Entities;
using Microsoft.Extensions.Configuration;
using Npgsql;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Cadastro.Data.Repositories
{
    public abstract class BaseRepository<T, U> : IRepository<T, U> where T : EntityBase<U>
    {
        protected readonly NpgsqlConnection connection;
        public BaseRepository(IConfiguration configuration)
        {
            connection = new NpgsqlConnection(configuration.GetConnectionString("Base"));
        }
        public abstract Task<IEnumerable<T>> RecuperarTodos();

        public abstract Task<T> RecuperarPorId(U id);

        public abstract Task<U> Inserir(T data);

        public abstract Task<bool> Atualizar(T data);
    }
}
