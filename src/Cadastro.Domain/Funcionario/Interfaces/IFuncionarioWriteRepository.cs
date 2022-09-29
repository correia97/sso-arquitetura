using Domain.Entities;
using System;
using System.Threading.Tasks;

namespace Cadastro.Domain.Interfaces
{
    public interface IFuncionarioWriteRepository : IWriteRepository<Funcionario, Guid>
    {
        Task<bool> InserirEndereco(Endereco endereco);
        Task<bool> AtualizarEndereco(Endereco endereco);
        Task<bool> RemoverEndereco(int id);
        Task<bool> InserirTelefone(Telefone telefone);
        Task<bool> AtualizarTelefone(Telefone telefone);
        Task<bool> RemoverTelefone(int id);
        Task<bool> Desativar(Guid id);
    }
}
