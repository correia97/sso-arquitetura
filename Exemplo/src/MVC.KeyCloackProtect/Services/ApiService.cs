using Flurl.Http;
using Microsoft.Extensions.Configuration;
using MVC.KeyCloackProtect.Interfaces;
using MVC.KeyCloackProtect.Models;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace MVC.KeyCloackProtect.Services
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
