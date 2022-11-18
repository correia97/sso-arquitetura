using Cadastro.Configuracoes;
using Cadastro.MVC.Interfaces;
using Cadastro.MVC.Models.Request;
using Cadastro.MVC.Models.Response;
using Flurl;
using Flurl.Http;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Polly;
using Polly.Retry;
using System;
using System.Collections.Generic;
using System.Net;
using System.Text.Json;
using System.Threading.Tasks;

namespace Cadastro.MVC.Services
{
    public class FuncionarioService : IFuncionarioService
    {
        private readonly string _baseUrl;
        private readonly AsyncRetryPolicy<IFlurlResponse> _policy;
        private readonly ILogger<FuncionarioService> _logger;
        public FuncionarioService(ILogger<FuncionarioService> logger, IConfiguration configuration)
        {
            _baseUrl = configuration.GetValue<string>("ServiceUrl");
            _logger = logger;

            var statusCodeRetry = new List<HttpStatusCode> {
                HttpStatusCode.BadRequest,
                HttpStatusCode.InternalServerError,
                HttpStatusCode.RequestTimeout,
                HttpStatusCode.ServiceUnavailable,
                HttpStatusCode.Unauthorized
            };

            _policy = Policy.HandleResult<IFlurlResponse>(result => statusCodeRetry.Contains((HttpStatusCode)result.StatusCode))
                .Or<Exception>()
                .WaitAndRetryAsync(3, retryAttempt => TimeSpan.FromSeconds(Math.Pow(2, retryAttempt)),
                        (responseOrException, timeSpan, retryCount, context) =>
                        {
                            if (responseOrException.Result?.StatusCode == 401)
                            {
                                _logger.CustomLogError($"Retry {retryCount} HTTP status 401");
                                throw new UnauthorizedAccessException();
                            }
                            if (responseOrException.Exception is not null)
                                _logger.CustomLogError(responseOrException.Exception, $"Retry {retryCount}");
                            else
                                _logger.CustomLogError($"Retry {retryCount} HTTP status {responseOrException.Result?.StatusCode}");

                        });
        }

        public async Task<Response<bool>> AtualizarFuncionario(FuncionarioRequest request, string token)
        {
            var result = await $"{_baseUrl}/api/v1/Funcionario/funcionario"
                                                                    .AllowAnyHttpStatus()
                                                                    .WithOAuthBearerToken(token)
                                                                    .PatchJsonAsync(request);
            if (result.StatusCode == (int)HttpStatusCode.OK)
                return Response<bool>.SuccessResult(true);
            if (result.StatusCode == (int)HttpStatusCode.Unauthorized)
                throw new UnauthorizedAccessException();
            return Response<bool>.ErrorResult(result.ResponseMessage.ReasonPhrase);
        }

        public async Task<Response<bool>> CadastrarFuncionario(FuncionarioRequest request, string token)
        {
            var result = await _policy.ExecuteAsync(() => $"{_baseUrl}/api/v1/Funcionario/funcionario"
                                                                    .AllowAnyHttpStatus()
                                                                    .WithOAuthBearerToken(token)
                                                                    .PostJsonAsync(request));
            if (result.StatusCode == (int)HttpStatusCode.OK)
                return Response<bool>.SuccessResult(true);
            if (result.StatusCode == (int)HttpStatusCode.Unauthorized)
                throw new UnauthorizedAccessException();
            return Response<bool>.ErrorResult(result.ResponseMessage.ReasonPhrase);
        }

        public async Task<Response<IEnumerable<FuncionarioResponse>>> ListarFuncionarios(string token, int pagina, int qtdItens)
        {
            var result = await _policy.ExecuteAsync(() => $"{_baseUrl}/api/v1/Funcionario/funcionario"
                                                                    .AppendPathSegment($"pagina/{pagina}")
                                                                    .AppendPathSegment($"qtdItens/{qtdItens}")
                                                                    .AllowAnyHttpStatus()
                                                                    .WithOAuthBearerToken(token)
                                                                    .GetAsync());
            if (result.StatusCode == (int)HttpStatusCode.OK)
            {
                var json = await result.ResponseMessage.Content.ReadAsStringAsync();
                return JsonSerializer.Deserialize<Response<IEnumerable<FuncionarioResponse>>>(json, new JsonSerializerOptions { PropertyNameCaseInsensitive = true });
            }
            if (result.StatusCode == (int)HttpStatusCode.Unauthorized)
                throw new UnauthorizedAccessException();
            return Response<IEnumerable<FuncionarioResponse>>.ErrorResult(result.ResponseMessage.ReasonPhrase);
        }

        public async Task<Response<FuncionarioResponse>> RecuperarFuncionario(Guid id, string token)
        {
            var result = await _policy.ExecuteAsync(() => $"{_baseUrl}/api/v1/Funcionario/funcionario"
                                                                    .AllowAnyHttpStatus()
                                                                    .WithOAuthBearerToken(token)
                                                                    .AppendPathSegment(id)
                                                                    .GetAsync());
            if (result.StatusCode == (int)HttpStatusCode.OK)
            {
                var json = await result.ResponseMessage.Content.ReadAsStringAsync();
                var response = JsonSerializer.Deserialize<Response<FuncionarioResponse>>(json, new JsonSerializerOptions { PropertyNameCaseInsensitive = true });

                return response;
            }
            if (result.StatusCode == (int)HttpStatusCode.Unauthorized)
                throw new UnauthorizedAccessException();
            return Response<FuncionarioResponse>.ErrorResult(result.ResponseMessage.ReasonPhrase);
        }

        public async Task<Response<bool>> RemoverFuncionario(Guid id, string token)
        {
            var result = await $"{_baseUrl}/api/v1/Funcionario/funcionario/{id}"
                                                                    .AllowAnyHttpStatus()
                                                                    .WithOAuthBearerToken(token)
                                                                    .DeleteAsync();
            if (result.StatusCode == (int)HttpStatusCode.OK)
                return Response<bool>.SuccessResult(true);
            if (result.StatusCode == (int)HttpStatusCode.Unauthorized)
                throw new UnauthorizedAccessException();
            return Response<bool>.ErrorResult(result.ResponseMessage.ReasonPhrase);
        }
    }
}
