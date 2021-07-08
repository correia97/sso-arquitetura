using Domain.Entities;
using System;
using System.Threading.Tasks;

namespace Cadastro.Domain.Interfaces
{
    public  interface IFuncionarioRepository : IRepository<Funcionario, Guid>
    {
        Task<Funcionario> BuscarPorEmail(string email);
    }
}
