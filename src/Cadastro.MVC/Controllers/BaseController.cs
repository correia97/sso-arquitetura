using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Http;
using System;
using System.Linq;
using System.Security.Claims;
using System.Threading.Tasks;

namespace MVC.Controllers
{
    public class BaseController : Microsoft.AspNetCore.Mvc.Controller
    {
        public string AccessToken { get; private set; }
        public string IdToken { get; private set; }
        public Guid UserId { get; private set; }
        public string UserRole { get; private set; }
        public bool IsAdmin
        {
            get
            {
                if (string.IsNullOrEmpty(UserRole) || UserRole.ToUpper() != "/ADMIN")
                    return false;
                return true;
            }
        }
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

                var roles = User.Claims.Where(x => x.Type == ClaimTypes.Role);
                if (roles != null)
                {
                    var role = roles.FirstOrDefault(x => x.Value.StartsWith("/"));
                    UserRole = role?.Value;
                }
            }
        }
    }
}
