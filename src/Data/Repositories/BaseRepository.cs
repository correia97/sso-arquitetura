using API.Interfaces;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Threading.Tasks;

using Npgsql;
using Microsoft.Extensions.Configuration;
using Dapper;
using Domain.Entities;

namespace API.Repositories
{
    public class BaseRepository<T, U> : IRepository<T, U> where T : EntityBase<U>
    {
        protected readonly NpgsqlConnection connection;
        public BaseRepository(IConfiguration configuration)
        {
            connection = new NpgsqlConnection(configuration.GetConnectionString("PostgresConnection"));
        }
        public Task<IEnumerable<T>> GetAll()
        {
            throw new NotImplementedException();
        }

        public Task<T> GetById(U id)
        {
            throw new NotImplementedException();
        }

        public Task<U> Insert(T data)
        {
            throw new NotImplementedException();
        }

        public Task<bool> Update(T data)
        {
            throw new NotImplementedException();
        }
    }
}
