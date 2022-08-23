using Domain.Entities;
using System.Data;
using System.Threading.Tasks;

namespace Cadastro.Domain.Interfaces
{
    public interface IWriteRepository<T, U> where T : EntityBase<U>
    {
        IDbConnection RecuperarConexao();
        Task<U> Inserir(T data, IDbConnection dbConnection, IDbTransaction transaction);
        Task<bool> Atualizar(T data, IDbConnection dbConnection, IDbTransaction transaction);
        Task<IDbTransaction> IniciarTransacao(IDbConnection dbConnection);
        Task CancelarTransacao(IDbConnection dbConnection, IDbTransaction transaction);
        Task CompletarTransacao(IDbConnection dbConnection, IDbTransaction transaction);
        Task DesalocarConexao(IDbConnection dbConnection);

    }
}
