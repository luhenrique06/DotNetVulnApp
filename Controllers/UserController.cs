using Microsoft.AspNetCore.Mvc;
using brokenaccesscontrol.Models;
using brokenaccesscontrol.Repositories;

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
    public async Task<ActionResult> Register([FromBody]UserRequest userRequest)
    {

        try{
            
            if (await UserRepository.LoginExist(userRequest.Login))
                return Conflict(
                    new {
                        user = userRequest,
                        message = "User exist!!"
                    }
                );

            var user = await UserRepository.Insert(userRequest);

            return Ok(new
            {
                user = user,
                message = user == null ? "Error" : "Success"
            });                     

        }catch(Exception ex){
            _logger.LogError(ex, "General error");
            return StatusCode(500, "Internal server error");            
        }


    }    

    [HttpPost]
    [Route("passwordrecovery")]
    public async Task<ActionResult> PasswordRecovery([FromBody]PasswordRecovery recovery){
        try{
            
            await UserRepository.RecoveryPassword(recovery);

            return Ok(new
            {
                message = "Caso seu login exista em nossa base de dados você receberá um e-mail com as instruções."
            });    
        }catch(Exception ex){
            _logger.LogError(ex, "General error");
            return StatusCode(500, "Internal server error");     
        }
    }     

    [HttpGet]
    public async Task<IEnumerable<User>> GetAllUsers()
    {
        return await UserRepository.GetAllUsers();
    }


}