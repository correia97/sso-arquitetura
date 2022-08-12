using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Http;
using System;
using System.Linq;
using System.Threading.Tasks;

namespace MVC.Controllers
{
    public class BaseController : Microsoft.AspNetCore.Mvc.Controller
    {
        public string AccessToken { get; private set; }

        public string IdToken { get; private set; }
        public Guid UserId { get; private set; }
        public BaseController()
        {
        }

        protected async Task GetTokens()
        {
            if (User != null && User.Identity.IsAuthenticated)
            {
                AccessToken = await HttpContext.GetTokenAsync("access_token");
                IdToken = await HttpContext.GetTokenAsync("id_token");
                var userId = User.Claims.FirstOrDefault(x => x.Type == "userId");
                if (userId != null)
                    UserId = Guid.Parse(userId.Value);
                // var expire = await HttpContext.GetTokenAsync("expires_at");
            }
        }
    }
}
