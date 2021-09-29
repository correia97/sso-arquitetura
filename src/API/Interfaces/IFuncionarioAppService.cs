using Domain.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Cadastro.API.Interfaces
{
    public interface IFuncionarioAppService
    {
        bool Cadastrar(Funcionario funcionario);
        bool Atualizar(Funcionario funcionario, string currentUserId);
        Task<Funcionario> ObterPorId(Guid id);
        Task<IEnumerable<Funcionario>> ObterTodos();
    }
}
