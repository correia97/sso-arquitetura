using Domain.Entities;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Cadastro.Domain.Interfaces
{
    public interface IFuncionarioService
    {
        Task<bool> Cadastrar(Funcionario funcionario);
        Task<bool> Atualizar(Funcionario funcionario, string currentUserId);
        Task<Funcionario> RecuperarPorId(Guid id);
        Task<IEnumerable<Funcionario>> RecuperarTodos();
    }
}
