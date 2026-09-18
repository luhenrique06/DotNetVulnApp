using brokenaccesscontrol.Models;
using brokenaccesscontrol.Repositories;
using brokenaccesscontrol.Utils;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace brokenaccesscontrol.Controllers;

[ApiController]
[Route("api/[controller]")]
[Authorize]
public class TransfersController : ControllerBase
{
    private readonly ILogger<TransfersController> _logger;

    public TransfersController(ILogger<TransfersController> logger)
    {
        _logger = logger;
    }

    // A06 - Insecure Design:
    //   * saldo lido e gravado em passos separados, sem transação nem lock ->
    //     requisições concorrentes causam double-spend / saldo negativo.
    //   * dailyLimit existe no cadastro mas NUNCA é checado (sem controle de velocidade).
    // A10 - Mishandling of Exceptional Conditions:
    //   * débito é aplicado antes do crédito; se o crédito falhar, não há rollback ->
    //     estado inconsistente (dinheiro some).
    [HttpPost]
    public async Task<ActionResult> Transfer([FromBody] TransferRequest req)
    {
        var userId = User.FindFirst("UserId")?.Value ?? "";

        var from = await AccountRepository.GetById(req.FromAccount);
        if (from == null || from.OwnerId != userId)
            return StatusCode(403, new { message = "Conta de origem inválida" });

        if (req.Amount <= 0)
            return BadRequest(new { message = "Valor inválido" });

        // Leitura do saldo (T0).
        var saldo = await AccountRepository.GetBalance(req.FromAccount);
        if (saldo < req.Amount)
            return BadRequest(new { message = "Saldo insuficiente" });

        // Janela de corrida: processamento assíncrono entre checagem e escrita.
        await Task.Delay(60);

        // Débito (sem transação/lock).
        await AccountRepository.SetBalance(req.FromAccount, saldo - req.Amount);

        // Crédito no destino. Se a conta destino não existir, o débito já ocorreu
        // e não é revertido (A10).
        var dest = await AccountRepository.GetById(req.ToAccount);
        if (dest == null)
            return StatusCode(500, new { message = "Falha ao creditar destino" });

        await AccountRepository.SetBalance(req.ToAccount, dest.Saldo + req.Amount);

        var id = await TransferRepository.Insert(new Transfer
        {
            FromAccount = req.FromAccount,
            ToAccount = req.ToAccount,
            Amount = req.Amount,
            Status = "completed",
            CreatedAt = DateTime.UtcNow.ToString("yyyy-MM-dd HH:mm:ss"),
            IdempotencyKey = req.IdempotencyKey
        });

        AccessLog.Info($"Transfer {req.Amount} from {req.FromAccount} to {req.ToAccount} by {userId}");
        return Ok(new { transferId = id, message = "Transferência concluída" });
    }
}
