using MVC.KeyCloakProtect.Models;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace MVC.KeyCloakProtect.Interfaces
{
    public  interface IApiService
    {
        Task<List<Forecast>> GetWeatherForecast(string authToken);
    }
}
