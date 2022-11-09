using Cadastro.API.Models.Response;
using Domain.Entities;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Cadastro.API.Interfaces
{
    public interface IFuncionarioAppService
    {
        bool Cadastrar(Funcionario funcionario, Guid correlationId);
        bool Atualizar(Funcionario funcionario, Guid correlationId);
        bool Desativar(Guid id, Guid correlationId);
        bool Remover(Guid id, Guid correlationId);
        Task<FuncionarioResponse> ObterPorId(Guid id);
        Task<(IEnumerable<FuncionarioResponse>, int)> ObterTodos(int pagina, int qtdItens);
    }
}
