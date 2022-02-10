using Domain.Entities;
using System;

namespace Cadastro.Domain.Interfaces
{
    public interface IFuncionarioWriteRepository : IWriteRepository<Funcionario, Guid>
    {
    }
}
