using brokenaccesscontrol.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json;

namespace brokenaccesscontrol.Controllers;

[ApiController]
[Route("api/[controller]")]
[Authorize]
public class ImportController : ControllerBase
{
    [HttpPost]
    [Route("restore")]
    public IActionResult Restore([FromBody] ImportRequest req)
    {
        try
        {
            var settings = new JsonSerializerSettings
            {
                TypeNameHandling = TypeNameHandling.None
            };
            var restored = JsonConvert.DeserializeObject<UserPreferences>(req.Backup ?? "{}", settings);
            return Ok(new { restored = restored?.GetType().FullName ?? "null" });
        }
        catch (Exception ex)
        {
            return StatusCode(500, new { error = ex.Message });
        }
    }
}
