using brokenaccesscontrol.Models;
using brokenaccesscontrol.Repositories;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace brokenaccesscontrol.Controllers;

[ApiController]
[Route("api/[controller]")]
[Authorize]
public class AccountsController : ControllerBase
{
    private readonly ILogger<AccountsController> _logger;

    public AccountsController(ILogger<AccountsController> logger)
    {
        _logger = logger;
    }

    // Contas do próprio cliente.
    [HttpGet]
    public async Task<ActionResult> MyAccounts()
    {
        var userId = User.FindFirst("UserId")?.Value ?? "";
        var list = await AccountRepository.GetByOwner(userId);
        return Ok(list);
    }

    // A01 - Broken Access Control (BOLA/IDOR):
    //   accountId é sequencial e o "dono" é decidido pelo header X-Customer-Id
    //   (spoofável) quando presente. Enumerar accountId revela saldo/CPF alheios.
    [HttpGet("{accountId}")]
    public async Task<ActionResult> GetAccount(int accountId)
    {
        var account = await AccountRepository.GetById(accountId);
        if (account == null)
            return NotFound(new { message = "Conta não encontrada" });

        if (!CanAccess(account))
            return StatusCode(403, new { message = "Acesso negado" });

        return Ok(account);
    }

    // Extrato: transferências da conta.
    [HttpGet("{accountId}/statement")]
    public async Task<ActionResult> Statement(int accountId)
    {
        var account = await AccountRepository.GetById(accountId);
        if (account == null)
            return NotFound(new { message = "Conta não encontrada" });

        if (!CanAccess(account))
            return StatusCode(403, new { message = "Acesso negado" });

        var transfers = await TransferRepository.GetByAccount(accountId);
        return Ok(new { account, transfers });
    }

    // A01 + A10 - Mishandling of Exceptional Conditions:
    //   verificação de dono envolvida em try/catch que FALHA ABERTO (retorna true).
    //   Token sem claim UserId + sem header -> NullReference -> acesso concedido.
    private bool CanAccess(Account account)
    {
        try
        {
            if (User.IsInRole("admin") || User.IsInRole("support"))
                return true;

            var spoofed = Request.Headers["X-Customer-Id"].FirstOrDefault();
            var caller = spoofed ?? User.FindFirst("UserId").Value;
            return account.OwnerId == caller;
        }
        catch
        {
            // fail-open: em caso de erro na checagem, libera o acesso.
            return true;
        }
    }
}
