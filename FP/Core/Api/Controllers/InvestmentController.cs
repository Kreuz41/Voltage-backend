using FP.Core.Api.ApiDto;
using FP.Core.Api.Helpers;
using FP.Core.Api.Providers.Interfaces;
using FP.Core.Api.Responses;
using FP.Core.Database.Handlers;
using FP.Core.Database.Models;
using Microsoft.AspNetCore.Mvc;

namespace FP.Core.Api.Controllers;

[ApiController]
[Route("api/investment")]
public class InvestmentController : ControllerBase
{
    private readonly JwtService _jwtService;
    private readonly ICryptoApiTrc20Provider _cryptoTRC20Provider;
    private readonly ILogger<InvestmentController> _logger;
    private readonly UserDatabaseHandler _userDatabaseHandler;
    private readonly InvestmentDatabaseHandler _investmentDatabaseHandler;

    public InvestmentController(JwtService jwtService, ILogger<InvestmentController> logger, UserDatabaseHandler userDatabaseHandler,
        ICryptoApiTrc20Provider cryptoApiTrc20Provider, InvestmentDatabaseHandler investmentDatabaseHandler)
    {
        _cryptoTRC20Provider = cryptoApiTrc20Provider;
        _userDatabaseHandler = userDatabaseHandler;
        _investmentDatabaseHandler = investmentDatabaseHandler;
        _jwtService = jwtService;
        _logger = logger;
    }
    
    [HttpPost("create/{code}")]
    public async Task<IActionResult> CreateInvestmentAsync(string code)
    {
        var jwt = Request.Cookies["jwt"];
        if (jwt == null)
            return Unauthorized();
        var token = _jwtService.Verify(jwt);
        if (token == null)
            return Unauthorized();
        var isSuccess = int.TryParse(token.Issuer, out var userId);
        if (!isSuccess)
            return BadRequest(new InvalidData("Token"));

        var investment = await _investmentDatabaseHandler.CreateInvestment(userId, code);

        return investment.Status ? Ok(investment) : BadRequest(investment);
    }
    
    [HttpGet("all")]
    public async Task<IActionResult> GetAllInvestmentAsync()
    {
        var jwt = Request.Cookies["jwt"];
        if (jwt == null)
            return Unauthorized();
        var token = _jwtService.Verify(jwt);
        if (token == null)
            return Unauthorized();
        var isSuccess = int.TryParse(token.Issuer, out var userId);
        if (!isSuccess)
            return BadRequest(new InvalidData("Token"));

        var response = await _investmentDatabaseHandler.GetAllInvestments(userId);
        
        return response.Status ? Ok(response) : BadRequest(response);
    }
}