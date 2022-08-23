using Domain.Entities;
using System;
using System.Data;
using System.Threading.Tasks;

namespace Cadastro.Domain.Interfaces
{
    public interface IFuncionarioWriteRepository : IWriteRepository<Funcionario, Guid>
    {
        Task<bool> InserirEndereco(Endereco endereco, IDbConnection dbConnection, IDbTransaction transaction);
        Task<bool> AtualizarEndereco(Endereco endereco, IDbConnection dbConnection, IDbTransaction transaction);
        Task<bool> RemoverEndereco(int id, IDbConnection dbConnection, IDbTransaction transaction);
        Task<bool> InserirTelefone(Telefone telefone, IDbConnection dbConnection, IDbTransaction transaction);
        Task<bool> AtualizarTelefone(Telefone telefone, IDbConnection dbConnection, IDbTransaction transaction);
        Task<bool> RemoverTelefone(int id, IDbConnection dbConnection, IDbTransaction transaction);
    }
}
