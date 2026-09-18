using brokenaccesscontrol.Models;
using brokenaccesscontrol.Repositories;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace brokenaccesscontrol.Controllers;

[ApiController]
[Route("api/[controller]")]
[Authorize]
public class CouponsController : ControllerBase
{
    // A06 - Insecure Design: check-then-act não atômico.
    //   usesLeft é verificado, o cashback é creditado, e só DEPOIS usesLeft é decrementado.
    //   Requisições concorrentes com o mesmo cupom passam todas na verificação (reuso).
    [HttpPost("apply")]
    public async Task<ActionResult> Apply([FromBody] CouponApplyRequest req)
    {
        var coupon = await CouponRepository.Get(req.Code ?? "");
        if (coupon == null || coupon.UsesLeft <= 0)
            return BadRequest(new { message = "Cupom inválido ou esgotado" });

        // Janela de corrida.
        await Task.Delay(50);

        var saldo = await AccountRepository.GetBalance(req.AccountId);
        var bonus = saldo * coupon.ValuePct / 100m;
        await AccountRepository.SetBalance(req.AccountId, saldo + bonus);

        await CouponRepository.DecrementUses(coupon.Code!);
        return Ok(new { message = "Cashback aplicado", bonus });
    }
}
