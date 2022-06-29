using Flurl.Http;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using MVC.Interfaces;
using MVC.Models;
using Newtonsoft.Json;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace MVC.Services
{
    public class WeatherForecastService : IWeatherForecastService
    {
        private readonly string ServiceUrl;
        private ILogger<WeatherForecastService> _logger;
        public WeatherForecastService(IConfiguration configuration, ILogger<WeatherForecastService> logger)
        {
            ServiceUrl = configuration.GetValue<string>("ServiceUrl");
            _logger = logger;
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
                _logger.LogError("GetWeatherForecast erro", ex);
                throw;
            }
        }
    }
}
