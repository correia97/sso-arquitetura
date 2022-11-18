using Cadastro.API.Models;
using Cadastro.API.Models.Response;
using Cadastro.Configuracoes;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Cryptography;

namespace Cadastro.API.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class WeatherForecastController : ControllerBase
    {
        private static readonly string[] Summaries = new[]
        {
            "Freezing", "Bracing", "Chilly", "Cool", "Mild", "Warm", "Balmy", "Hot", "Sweltering", "Scorching"
        };

        private readonly ILogger<WeatherForecastController> _logger;

        public WeatherForecastController(ILogger<WeatherForecastController> logger)
        {
            _logger = logger;
        }

        [HttpGet]
        [Route("weatherForecast")]
        public Response<IEnumerable<WeatherForecast>> Get()
        {
            _logger.CustomLogInformation($"GET weatherForecast User {this.User?.Identity?.Name}");

            return Response<IEnumerable<WeatherForecast>>.SuccessResult(Enumerable.Range(1, 5).Select(index => new WeatherForecast
            {
                Date = DateTime.Now.AddDays(index),
                TemperatureC = RandomNumberGenerator.GetInt32(-20, 55),
                Summary = Summaries[RandomNumberGenerator.GetInt32(Summaries.Length)]
            })
            .ToArray(), 0);
        }

        [HttpPost]
        [Route("weatherForecast")]
        public Response<IEnumerable<WeatherForecast>> Post()
        {
            _logger.CustomLogInformation($"POST weatherForecast User {this.User?.Identity?.Name}");

            return Response<IEnumerable<WeatherForecast>>.SuccessResult(Enumerable.Range(1, 5).Select(index => new WeatherForecast
            {
                Date = DateTime.Now.AddDays(index),
                TemperatureC = RandomNumberGenerator.GetInt32(-20, 55),
                Summary = Summaries[RandomNumberGenerator.GetInt32(Summaries.Length)]
            })
            .ToArray(), 0);
        }

        [HttpGet]
        [Authorize]
        [Route("authorization")]
        public Response<IEnumerable<WeatherForecast>> GetWithAutorization()
        {
            _logger.CustomLogInformation($"GET authorization User {this.User?.Identity?.Name}");

            return Response<IEnumerable<WeatherForecast>>.SuccessResult(Enumerable.Range(1, 5).Select(index => new WeatherForecast
            {
                Date = DateTime.Now.AddDays(index),
                TemperatureC = RandomNumberGenerator.GetInt32(-20, 55),
                Summary = Summaries[RandomNumberGenerator.GetInt32(Summaries.Length)]
            })
            .ToArray(), 0);
        }
    }
}
