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
    // A08 - Software and Data Integrity Failures (deserialização insegura).
    //   "Restaurar backup de preferências" desserializa JSON controlado pelo usuário
    //   com TypeNameHandling.All -> o campo $type instancia tipos arbitrários.
    //   Gadget disponível: brokenaccesscontrol.Utils.FileLog (grava arquivo no ctor
    //   e em [OnDeserialized]) -> escrita de arquivo em caminho controlado.
    //
    //   Ex. payload em Backup:
    //   {"$type":"brokenaccesscontrol.Utils.FileLog, brokenaccesscontrol","path":"/tmp/pwned.txt"}
    [HttpPost]
    [Route("restore")]
    public IActionResult Restore([FromBody] ImportRequest req)
    {
        try
        {
            var settings = new JsonSerializerSettings
            {
                TypeNameHandling = TypeNameHandling.All
            };
            var restored = JsonConvert.DeserializeObject(req.Backup ?? "{}", settings);
            return Ok(new { restored = restored?.GetType().FullName ?? "null" });
        }
        catch (Exception ex)
        {
            return StatusCode(500, new { error = ex.Message });
        }
    }
}
