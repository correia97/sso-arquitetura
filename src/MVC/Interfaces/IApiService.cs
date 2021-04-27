using MVC.Models;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace MVC.Interfaces
{
    public  interface IApiService
    {
        Task<List<Forecast>> GetWeatherForecast(string authToken);
    }
}
