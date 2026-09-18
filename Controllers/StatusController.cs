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

        // A02 - Security Misconfiguration:
        //   endpoint de "health/debug" sem autenticação expõe variáveis de ambiente
        //   (inclui JWTSecret, SqliteDatabase), versão e host. Reconhecimento fácil.
        [HttpGet("debug")]
        public IActionResult Debug()
        {
            var env = new Dictionary<string, string?>();
            foreach (System.Collections.DictionaryEntry e in Environment.GetEnvironmentVariables())
                env[e.Key.ToString()!] = e.Value?.ToString();

            return Ok(new
            {
                environment = Environment.GetEnvironmentVariable("ASPNETCORE_ENVIRONMENT"),
                framework = Environment.Version.ToString(),
                machine = Environment.MachineName,
                jwtSecret = Environment.GetEnvironmentVariable("JWTSecret"),
                database = Environment.GetEnvironmentVariable("SqliteDatabase"),
                variables = env
            });
        }
    }
}
