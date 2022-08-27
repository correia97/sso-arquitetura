using Domain.Entities;
using System.Data;
using System.Threading.Tasks;

namespace Cadastro.Domain.Interfaces
{
    public interface IWriteRepository<T, U> where T : EntityBase<U>
    {
        IDbConnection RecuperarConexao();
        Task<U> Inserir(T data, IDbTransaction transaction);
        Task<bool> Atualizar(T data, IDbTransaction transaction);
        IDbTransaction IniciarTransacao();
        void CancelarTransacao(IDbTransaction transaction);
        void CompletarTransacao(IDbTransaction transaction);

    }
}
