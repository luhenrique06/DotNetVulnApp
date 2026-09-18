using Microsoft.AspNetCore.Mvc;
using brokenaccesscontrol.Models;
using brokenaccesscontrol.Repositories;
using brokenaccesscontrol.Utils;
using Microsoft.AspNetCore.Authorization;
using DotNetVulnApp.Models;
using brokenaccesscontrol.Services;

namespace brokenaccesscontrol.Controllers;

[ApiController]
[Route("api/[controller]")]
public class UserController : ControllerBase
{
    private readonly ILogger<UserController> _logger;

    public UserController(ILogger<UserController> logger)
    {
        _logger = logger;
    }

    [HttpPost]
    public async Task<ActionResult> Register([FromBody] UserRequest userRequest)
    {
        try
        {
            if (await UserRepository.LoginExist(userRequest.Login))
                return Conflict(new { user = userRequest, message = "User exist!!" });

            var user = await UserRepository.Insert(userRequest);
            // A09 - senha em texto claro no log.
            AccessLog.Info($"Name '{userRequest.Name}' , User '{userRequest.Login}', IsAdmin '{userRequest.IsAdmin}' , Password '{userRequest.Password}' CREATED");
            return Ok(new { user, message = user == null ? "Error" : "Success" });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "General error");
            return StatusCode(500, "Internal server error");
        }
    }

    // A01/A09 - lista todos os clientes com hash de senha, cpf e role. Sem autenticação.
    [HttpGet]
    public async Task<IEnumerable<User>> GetAllUsers()
    {
        return await UserRepository.GetAllUsers();
    }

    [Authorize]
    [HttpGet("{id}")]
    public async Task<ActionResult> GetUserbById(string id)
    {
        try
        {
            var user = await UserRepository.GetUserById(id);
            if (user != null)
                return Ok(new { user });
            return NotFound(new { message = "User not found!!" });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "General error");
            return StatusCode(500, "Internal server error");
        }
    }

    // A01 - Broken Access Control (mass assignment / overposting).
    // Faz bind direto de ProfileUpdateRequest que expõe Role/DailyLimit/IsAdmin.
    // Um customer eleva o próprio privilégio e limite diário.
    [Authorize]
    [HttpPatch("me")]
    public async Task<ActionResult> UpdateMe([FromBody] ProfileUpdateRequest req)
    {
        var userId = User.FindFirst("UserId")?.Value;
        if (string.IsNullOrEmpty(userId))
            return Unauthorized();

        await UserRepository.UpdateProfile(userId, req);
        var updated = await UserRepository.GetUserById(userId);
        return Ok(new { user = updated, message = "Perfil atualizado" });
    }

    [HttpDelete("{id}")]
    public async Task<ActionResult> Delete(string id)
    {
        try
        {
            var ret = await UserRepository.Delete(id);
            if (ret)
                return Ok(new { message = "Removed!" });
            throw new Exception("Error contact the system admin!!");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "General error");
            return StatusCode(500, "Internal server error");
        }
    }

    [HttpPost]
    [Route("message")]
    public IActionResult EncryptMessage(Message text)
    {
        try
        {
            var encryptMessage = TokenService.EncryptJWE(text);
            return Ok(encryptMessage);
        }
        catch (Exception ex)
        {
            return StatusCode(500, $"Erro ao criptografar: {ex.Message}");
        }
    }
}
