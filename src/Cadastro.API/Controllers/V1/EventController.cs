using Microsoft.AspNetCore.Mvc;
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
        public IActionResult SendEvent<T>(T even)
        {
            return Ok(even);
        }
    }
}
