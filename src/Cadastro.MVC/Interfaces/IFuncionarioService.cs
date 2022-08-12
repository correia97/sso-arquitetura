using Cadastro.MVC.Models.Request;
using Cadastro.MVC.Models.Response;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Cadastro.MVC.Interfaces
{
    public interface IFuncionarioService
    {
        Task<Response<FuncionarioResponse>> RecuperarFuncionario(Guid idtoken, string token);
        Task<Response<IEnumerable<FuncionarioResponse>>> ListarFuncionarios(string token);
        Task<Response<bool>> CadastrarFuncionario(FuncionarioRequest requesttoken, string token);
        Task<Response<bool>> AtualizarFuncionario(FuncionarioRequest requesttokentoken, string token);
    }
}
