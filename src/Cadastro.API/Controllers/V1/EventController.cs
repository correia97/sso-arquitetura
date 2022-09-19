using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using System.Threading.Tasks;

namespace Cadastro.API.Controllers.V1
{
    [Route("api/[controller]")]
    [ApiController]
    public class EventController : ControllerBase
    {
        public EventController()
        {

        }

        [HttpGet]
        public async Task<IActionResult> SendEvent<T>(T even)
        {
            return Ok(even);
        }
    }
}
