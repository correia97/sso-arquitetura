using Domain.Entities;
using System;
using System.Collections.Generic;
using System.Data;
using System.Threading.Tasks;

namespace Cadastro.Domain.Interfaces
{
    public interface IFuncionarioReadRepository : IReadRepository<Funcionario, Guid>
    {
        Task<Funcionario> ObterPorEmail(IDbTransaction transaction, string email);
        Task<List<Telefone>> ObterTelefonesPorFuncionarioId(IDbTransaction transaction, Guid funcionarioId);
        Task<List<Endereco>> ObterEnderecosPorFuncionarioId(IDbTransaction transaction, Guid funcionarioId);
    }
}
