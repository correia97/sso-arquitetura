using MVC.KeyCloackProtect.Models;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace MVC.KeyCloackProtect.Interfaces
{
    public  interface IApiService
    {
        Task<List<Forecast>> GetWeatherForecast(string authToken);
    }
}
