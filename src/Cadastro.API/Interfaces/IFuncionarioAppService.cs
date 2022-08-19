using Cadastro.API.Models.Response;
using Domain.Entities;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Cadastro.API.Interfaces
{
    public interface IFuncionarioAppService
    {
        bool Cadastrar(Funcionario funcionario);
        bool Atualizar(Funcionario funcionario, string currentUserId);
        Task<FuncionarioResponse> ObterPorId(Guid id);
        Task<IEnumerable<FuncionarioResponse>> ObterTodos();
    }
}
