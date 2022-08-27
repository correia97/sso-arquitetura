using Domain.Entities;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Cadastro.Domain.Services
{
    public interface IFuncionarioService
    {
        Task Cadastrar(Funcionario funcionario);
        Task Atualizar(Funcionario funcionario);
        Task<Funcionario> ObterPorId(Guid id);
        Task<IEnumerable<Funcionario>> ObterTodos();
    }
}
