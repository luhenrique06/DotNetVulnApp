using brokenaccesscontrol.Models;
using brokenaccesscontrol.Repositories;
using brokenaccesscontrol.Services;
using brokenaccesscontrol.Utils;
using Microsoft.AspNetCore.Mvc;

namespace brokenaccesscontrol.Controllers;

[ApiController]
[Route("api/[controller]")]
public class AuthenticationController : ControllerBase
{
    private readonly ILogger<AuthenticationController> _logger;

    public AuthenticationController(ILogger<AuthenticationController> logger)
    {
        _logger = logger;
    }

    // A07 - Authentication Failures:
    //   * sem rate-limit / lockout -> brute force e credential stuffing livres.
    //   * MFA (mfaSecret existe no cadastro) é "verificado" via flag do próprio cliente.
    // A09 - senha em texto claro no log de acesso.
    // A01 - login.IsAdmin permite mass-assignment de privilégio.
    [HttpPost]
    [Route("login")]
    public async Task<ActionResult> Login([FromBody] LoginRequest login)
    {
        var user = await UserRepository.Login(login);

        if (user == null)
        {
            AccessLog.Error($"User '{login.Login}' Password '{login.Password}' ERROR");
            return Unauthorized(new { message = "User not found!" });
        }

        if (user.Password == UtilService.ReturnMD5(login.Password ?? ""))
        {
            // A07 - se o usuário tem MFA, deveria exigir código TOTP validado no servidor.
            // Aqui basta o cliente enviar MfaVerified=true (confiança client-side).
            if (!string.IsNullOrEmpty(user.MfaSecret) && login.MfaVerified != true)
            {
                return Unauthorized(new { message = "MFA required", mfa = true });
            }

            // A01 - escalonamento: o cliente decide se é admin.
            if (login.IsAdmin.HasValue)
                user.IsAdmin = login.IsAdmin.Value;

            var token = TokenService.GenerateToken(user);
            AccessLog.Info($"User '{login.Login}' Password '{login.Password}' logged");
            user.Password = null;
            return Ok(new { User = user, token });
        }

        AccessLog.Error($"User '{login.Login}' Password '{login.Password}'");
        return Unauthorized(new { message = "Wrong password!!!" });
    }

    // A05 - Injection: login/senha concatenados na query (ver UserRepository.LoginSQL).
    [HttpPost]
    [Route("loginsql")]
    public async Task<ActionResult> LoginSQL([FromBody] LoginRequest login)
    {
        var user = await UserRepository.LoginSQL(login);
        if (user == null)
            return Unauthorized(new { message = "User not found!" });

        var token = TokenService.GenerateToken(user);
        AccessLog.Info($"User '{login.Login}' Password '{login.Password}' logged (sql)");
        return Ok(new { User = user, token });
    }

    // A06 - Insecure Design: "recuperação" apenas inativa a conta do login informado
    // e grava um token previsível. Sem verificar identidade -> DoS + prep de takeover.
    [HttpPost]
    [Route("passwordrecovery")]
    public async Task<ActionResult> PasswordRecovery([FromBody] PasswordRecovery recovery)
    {
        await UserRepository.RecoveryPassword(recovery);
        return Ok(new { message = "Caso seu login exista em nossa base você receberá instruções." });
    }

    // A04/A06 - reset aceita token previsível (MD5(login+yyyyMMdd)) e não confirma dono.
    [HttpPost]
    [Route("passwordreset")]
    public async Task<ActionResult> ResetPassword([FromBody] PasswordReset reset)
    {
        var ok = await UserRepository.ResetPassword(reset);
        if (!ok)
            return BadRequest(new { message = "Token inválido" });
        return Ok(new { message = "Senha alterada" });
    }
}
