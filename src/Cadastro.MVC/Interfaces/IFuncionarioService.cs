using Cadastro.MVC.Models.Request;
using Cadastro.MVC.Models.Response;
using System.Collections.Generic;

namespace Cadastro.MVC.Interfaces
{
    public interface IFuncionarioService
    {

        Response<FuncionarioResponse> RecuperarFuncionario();
        Response<IEnumerable<FuncionarioResponse>> ListarFuncionarios();
        Response<bool> CadastrarFuncionario(FuncionarioRequest request);
        Response<bool> AtualizarFuncionario(FuncionarioRequest request);
    }
}
