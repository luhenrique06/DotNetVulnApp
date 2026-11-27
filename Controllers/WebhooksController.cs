using System.Security.Cryptography;
using System.Text;
using brokenaccesscontrol.Models;
using brokenaccesscontrol.Repositories;
using brokenaccesscontrol.Utils;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace brokenaccesscontrol.Controllers;

[ApiController]
[Route("api/[controller]")]
public class WebhooksController : ControllerBase
{
    private static readonly HttpClient _http = new HttpClient();

    // SSRF (Server-Side Request Forgery):
    //   ao registrar o webhook o servidor faz uma requisição de "validação" para a URL
    //   informada e devolve o corpo/status ao chamador. Sem allowlist nem bloqueio de
    //   IPs internos -> leitura de metadados de nuvem (169.254.169.254), localhost, etc.
    [Authorize]
    [HttpPost("register")]
    public async Task<ActionResult> Register([FromBody] WebhookRegisterRequest req)
    {
        var userId = User.FindFirst("UserId")?.Value ?? "";
        if (string.IsNullOrEmpty(req.CallbackUrl))
            return BadRequest(new { message = "callbackUrl obrigatório" });

        string body;
        int status;
        try
        {
            var resp = await _http.GetAsync(req.CallbackUrl);
            status = (int)resp.StatusCode;
            body = await resp.Content.ReadAsStringAsync();
        }
        catch (Exception ex)
        {
            status = 0;
            body = ex.Message;
        }

        var id = await WebhookRepository.Insert(userId, req.CallbackUrl, req.Secret);
        return Ok(new { id, validation = new { status, body } });
    }

    // A08 - Software and Data Integrity Failures:
    //   callback de confirmação de PIX credita a conta destino somente após verificar a
    //   assinatura HMAC do payload contra o secret registrado.
    [HttpPost("pix-callback")]
    public async Task<ActionResult> PixCallback([FromBody] WebhookCallback cb)
    {
        // Retrieve the registered webhook secret for signature validation.
        var webhook = await WebhookRepository.GetByExternalId(cb.ExternalId);
        if (webhook == null)
            return Unauthorized(new { message = "Webhook não registrado" });

        if (string.IsNullOrEmpty(webhook.Secret))
            return Unauthorized(new { message = "Webhook sem secret configurado" });

        // Validate the HMAC-SHA256 signature of the payload.
        if (string.IsNullOrEmpty(cb.Signature))
            return Unauthorized(new { message = "Assinatura ausente" });

        var payload = $"{cb.ToAccount}:{cb.Amount}:{cb.ExternalId}";
        var secretBytes = Encoding.UTF8.GetBytes(webhook.Secret);
        var payloadBytes = Encoding.UTF8.GetBytes(payload);

        using var hmac = new HMACSHA256(secretBytes);
        var expectedHashBytes = hmac.ComputeHash(payloadBytes);
        var expectedSignature = Convert.ToHexString(expectedHashBytes).ToLowerInvariant();

        // Use constant-time comparison to prevent timing attacks.
        if (!CryptographicOperations.FixedTimeEquals(
                Encoding.UTF8.GetBytes(expectedSignature),
                Encoding.UTF8.GetBytes(cb.Signature.ToLowerInvariant())))
        {
            AccessLog.Warn($"PIX callback signature mismatch for ext={cb.ExternalId} toAccount={cb.ToAccount}");
            return Unauthorized(new { message = "Assinatura inválida" });
        }

        var dest = await AccountRepository.GetById(cb.ToAccount);
        if (dest == null)
            return NotFound(new { message = "Conta destino não encontrada" });

        await AccountRepository.SetBalance(cb.ToAccount, dest.Saldo + cb.Amount);
        AccessLog.Info($"PIX callback credited {cb.Amount} to {cb.ToAccount} ext={cb.ExternalId}");
        return Ok(new { message = "Crédito confirmado" });
    }
}
