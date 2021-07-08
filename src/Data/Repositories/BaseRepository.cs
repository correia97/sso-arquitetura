using Cadastro.Domain.Interfaces;
using Domain.Entities;
using Microsoft.Extensions.Configuration;
using Npgsql;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace API.Repositories
{
    public class BaseRepository<T, U> : IRepository<T, U> where T : EntityBase<U>
    {
        protected readonly NpgsqlConnection connection;
        public BaseRepository(IConfiguration configuration)
        {
            connection = new NpgsqlConnection(configuration.GetConnectionString("PostgresConnection"));
        }
        public Task<IEnumerable<T>> RecuperarTodos()
        {
            throw new NotImplementedException();
        }

        public Task<T> RecuperarPorId(U id)
        {
            throw new NotImplementedException();
        }

        public Task<U> Inserir(T data)
        {
            throw new NotImplementedException();
        }

        public Task<bool> Atualizar(T data)
        {
            throw new NotImplementedException();
        }
    }
}
