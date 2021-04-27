using Flurl.Http;
using Microsoft.Extensions.Configuration;
using MVC.Interfaces;
using MVC.Models;
using Newtonsoft.Json;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace MVC.Services
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
            try
            {
                var result = await $"{ServiceUrl}/api/weatherforecast/authorization".WithOAuthBearerToken(authToken)
                                 .AllowAnyHttpStatus()
                                 .GetAsync();
                if (result.ResponseMessage.IsSuccessStatusCode)
                {
                    return JsonConvert.DeserializeObject<List<Forecast>>(await result.ResponseMessage.Content.ReadAsStringAsync());
                }
                else
                {
                    return null;
                }
            }
            catch (System.Exception ex)
            {

                throw ex;
            }
        }
    }
}
