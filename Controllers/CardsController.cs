using brokenaccesscontrol.Repositories;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace brokenaccesscontrol.Controllers;

[ApiController]
[Route("api/[controller]")]
[Authorize]
public class CardsController : ControllerBase
{
    // A04 - Cryptographic Failures: PAN e CVV retornados em texto claro (nunca deveriam
    // sair, muito menos sem tokenização/mascaramento).
    [HttpGet]
    public async Task<ActionResult> MyCards()
    {
        var userId = User.FindFirst("UserId")?.Value ?? "";
        var cards = await CardRepository.GetByOwner(userId);
        return Ok(cards);
    }

    // Fixed A01 - BOLA: Now checks that the authenticated user owns the card before returning it.
    [HttpGet("{id}")]
    public async Task<ActionResult> GetCard(int id)
    {
        var userId = User.FindFirst("UserId")?.Value ?? "";
        var card = await CardRepository.GetById(id);
        if (card == null)
            return NotFound(new { message = "Cartão não encontrado" });
        if (card.OwnerId != userId)
            return Forbid();
        return Ok(card);
    }
}
