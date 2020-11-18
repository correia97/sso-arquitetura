using Flurl.Http;
using Microsoft.Extensions.Configuration;
using MVC.KeyCloakProtect.Interfaces;
using MVC.KeyCloakProtect.Models;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace MVC.KeyCloakProtect.Services
{
    public class ApiService : IApiService
    {
        private readonly string ServiceUrl;
        public ApiService(IConfiguration configuration)
        {
            ServiceUrl = configuration.GetValue<string>("ServiceUrl");
        }
        public async Task<List<Forecast>> GetWeatherForecast(string authToken)
        {
            return await $"{ServiceUrl}/api/weatherforecast/authorization".WithOAuthBearerToken(authToken)
                 .AllowAnyHttpStatus()
                 .GetJsonAsync<List<Forecast>>();
        }
    }
}
