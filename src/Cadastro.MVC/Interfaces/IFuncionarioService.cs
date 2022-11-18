using Cadastro.MVC.Models.Request;
using Cadastro.MVC.Models.Response;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Cadastro.MVC.Interfaces
{
    public interface IFuncionarioService
    {
        Task<Response<FuncionarioResponse>> RecuperarFuncionario(Guid id, string token);
        Task<Response<IEnumerable<FuncionarioResponse>>> ListarFuncionarios(string token, int pagina, int qtdItens);
        Task<Response<bool>> CadastrarFuncionario(FuncionarioRequest request, string token);
        Task<Response<bool>> AtualizarFuncionario(FuncionarioRequest request, string token);
        Task<Response<bool>> RemoverFuncionario(Guid id, string token);
    }
}
