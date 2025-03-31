using Microsoft.AspNetCore.Mvc;

namespace brokenaccesscontrol.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class StatusController : ControllerBase
    {
        [HttpGet]
        public IActionResult Get()
        {
            return Ok("Está tudo ok");
        }
    }
}
