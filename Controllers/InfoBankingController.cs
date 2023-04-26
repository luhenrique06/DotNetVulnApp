using System.Security.Claims;
using brokenaccesscontrol.Models;
using brokenaccesscontrol.Repositories;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;


namespace brokenaccesscontrol.Controllers;

[ApiController]
[Route("api/[controller]")]
public class InfoBankingController : ControllerBase
{

    private readonly ILogger<InfoBankingController> _logger;

    public InfoBankingController(ILogger<InfoBankingController> logger)
    {
        _logger = logger;
    }   


    [HttpGet]
    public async Task<IEnumerable<InfoBanking>> GetAllInfoBanking()
    {
        return await InfoBankingRepository.GetAllInfoBanking();
    }

}