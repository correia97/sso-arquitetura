using Domain.Entities;
using System;
using System.Collections.Generic;
using System.Data;
using System.Threading.Tasks;

namespace Cadastro.Domain.Interfaces
{
    public interface IFuncionarioReadRepository : IReadRepository<Funcionario, Guid>
    {
        Task<Funcionario> ObterPorEmail(IDbConnection dbConnection, IDbTransaction transaction, string email);
        Task<List<Telefone>> ObterTelefonesPorFuncionarioId(IDbConnection dbConnection, IDbTransaction transaction, Guid funcionarioId);
        Task<List<Endereco>> ObterEnderecosPorFuncionarioId(IDbConnection dbConnection, IDbTransaction transaction, Guid funcionarioId);
    }
}
