using Cadastro.MVC.Models.Response;
using Flurl.Http;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using MVC.Interfaces;
using MVC.Models;
using System;
using System.Collections.Generic;
using System.Text.Json;
using System.Threading.Tasks;

namespace MVC.Services
{
    public class WeatherForecastService : IWeatherForecastService
    {
        private readonly string ServiceUrl;
        private readonly ILogger<WeatherForecastService> _logger;
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
                    var response = JsonSerializer.Deserialize<Response<List<Forecast>>>(await result.ResponseMessage.Content.ReadAsStringAsync(), new JsonSerializerOptions { PropertyNameCaseInsensitive = true });
                    return response.Data;
                }


                return null;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "GetWeatherForecast erro");
                throw;
            }
        }
    }
}
