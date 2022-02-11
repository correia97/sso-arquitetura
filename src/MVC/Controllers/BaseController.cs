using Microsoft.AspNetCore.Authentication;
using System.Threading.Tasks;

namespace MVC.Controllers
{
    public class BaseController : Microsoft.AspNetCore.Mvc.Controller
    {
        public string AccessToken { get; private set; }

        public string IdToken { get; private set; }
        public BaseController()
        {

            GetTokens().Wait();
        }

        private async Task GetTokens()
        {
            if (User != null && User.Identity.IsAuthenticated)
            {
                AccessToken = await HttpContext.GetTokenAsync("access_token");
                IdToken = await HttpContext.GetTokenAsync("id_token");
               // var expire = await HttpContext.GetTokenAsync("expires_at");
            }
        }
    }
}
