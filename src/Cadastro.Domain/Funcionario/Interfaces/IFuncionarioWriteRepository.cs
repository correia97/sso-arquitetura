using Domain.Entities;
using System;
using System.Data;
using System.Threading.Tasks;

namespace Cadastro.Domain.Interfaces
{
    public interface IFuncionarioWriteRepository : IWriteRepository<Funcionario, Guid>
    {
        Task<bool> InserirEndereco(Endereco endereco, IDbTransaction transaction);
        Task<bool> AtualizarEndereco(Endereco endereco, IDbTransaction transaction);
        Task<bool> RemoverEndereco(int id, IDbTransaction transaction);
        Task<bool> InserirTelefone(Telefone telefone, IDbTransaction transaction);
        Task<bool> AtualizarTelefone(Telefone telefone, IDbTransaction transaction);
        Task<bool> RemoverTelefone(int id, IDbTransaction transaction);
    }
}
