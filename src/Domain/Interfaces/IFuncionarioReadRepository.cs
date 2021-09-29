using Domain.Entities;
using System;
using System.Threading.Tasks;

namespace Cadastro.Domain.Interfaces
{
    public interface IFuncionarioReadRepository : IReadRepository<Funcionario, Guid>
    {
        Task<Funcionario> ObterPorEmail(string email);
    }
}
