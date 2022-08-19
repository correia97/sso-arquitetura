using Domain.Entities;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Cadastro.Domain.Interfaces
{
    public interface IFuncionarioReadRepository : IReadRepository<Funcionario, Guid>
    {
        Task<Funcionario> ObterPorEmail(string email);
        Task<List<Telefone>> ObterTelefonesPorFuncionarioId(Guid funcionarioId);
        Task<List<Endereco>> ObterEnderecosPorFuncionarioId(Guid funcionarioId);
    }
}
