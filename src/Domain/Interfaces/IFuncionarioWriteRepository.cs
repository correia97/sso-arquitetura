using Domain.Entities;
using System;
using System.Threading.Tasks;

namespace Cadastro.Domain.Interfaces
{
    public interface IFuncionarioWriteRepository : IWriteRepository<Funcionario, Guid>
    {
    }
}
