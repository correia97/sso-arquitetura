using Cadastro.MVC.Interfaces;
using Cadastro.MVC.Models.Request;
using Cadastro.MVC.Models.Response;
using System;
using System.Collections.Generic;
using Flurl;
using Flurl.Http;
using Microsoft.Extensions.Configuration;
using IdentityModel;
using System.Threading.Tasks;
using Azure.Core;
using System.Text.Json;

namespace Cadastro.MVC.Services
{
    public class FuncionarioService : IFuncionarioService
    {
        private readonly string BaseUrl;
        public FuncionarioService(IConfiguration configuration)
        {
            BaseUrl = configuration.GetValue<string>("ServiceUrl");
        }
        public async Task<Response<bool>> AtualizarFuncionario(FuncionarioRequest request, string token)
        {
            var result = await $"{BaseUrl}/api/v1/Funcionario/funcionario"
                                                                    .AllowAnyHttpStatus()
                                                                    .WithOAuthBearerToken(token)
                                                                    .PatchJsonAsync(request);
            if (result.ResponseMessage.IsSuccessStatusCode)
                return Response<bool>.SuccessResult(true);
            return Response<bool>.ErrorResult(result.ResponseMessage.ReasonPhrase);
        }

        public async Task<Response<bool>> CadastrarFuncionario(FuncionarioRequest request, string token)
        {
            var result = await $"{BaseUrl}/api/v1/Funcionario/funcionario"
                                                                    .AllowAnyHttpStatus()
                                                                    .WithOAuthBearerToken(token)
                                                                    .PostJsonAsync(request);
            if (result.ResponseMessage.IsSuccessStatusCode)
                return Response<bool>.SuccessResult(true);
            return Response<bool>.ErrorResult(result.ResponseMessage.ReasonPhrase);
        }

        public async Task<Response<IEnumerable<FuncionarioResponse>>> ListarFuncionarios( string token)
        {
            var result = await $"{BaseUrl}/api/v1/Funcionario/funcionario"
                                                                    .AllowAnyHttpStatus()
                                                                    .WithOAuthBearerToken(token)
                                                                    .GetAsync();
            if (result.ResponseMessage.IsSuccessStatusCode)
                return Response<IEnumerable<FuncionarioResponse>>
                    .SuccessResult(JsonSerializer.Deserialize<IEnumerable<FuncionarioResponse>>(await result.ResponseMessage.Content.ReadAsStringAsync()));
            return Response<IEnumerable<FuncionarioResponse>>.ErrorResult(result.ResponseMessage.ReasonPhrase);
        }

        public async Task<Response<FuncionarioResponse>> RecuperarFuncionario(Guid id, string token)
        {
            var result = await $"{BaseUrl}/api/v1/Funcionario/funcionario"
                                                                    .AllowAnyHttpStatus()
                                                                    .WithOAuthBearerToken(token)
                                                                    .AppendPathSegment(id)
                                                                    .GetAsync();
            if (result.ResponseMessage.IsSuccessStatusCode)
                return Response<FuncionarioResponse>
                    .SuccessResult(JsonSerializer.Deserialize<FuncionarioResponse>(await result.ResponseMessage.Content.ReadAsStringAsync()));
            return Response<FuncionarioResponse>.ErrorResult(result.ResponseMessage.ReasonPhrase);
        }
    }
}
