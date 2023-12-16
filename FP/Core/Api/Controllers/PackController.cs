using FP.Core.Api.ApiDto;
using FP.Core.Api.Helpers;
using FP.Core.Api.Responses;
using FP.Core.Database.Handlers;
using FP.Core.Database.Models;
using Microsoft.AspNetCore.Mvc;

namespace FP.Core.Api.Controllers;

[ApiController]
[Route("api/pack")]
public class PackController : ControllerBase
{
	private readonly ILogger<PackController> _logger;
	private readonly JwtService _jwtService;
	private readonly PackDatabaseHandler _databaseHandler;

	public PackController(PackDatabaseHandler databaseHandler, ILogger<PackController> logger, 
		JwtService jwtService)
	{
		_databaseHandler = databaseHandler;
		_logger = logger;
		_jwtService = jwtService;
	}

	[HttpGet("investCode/{code}")]
	public async Task<IActionResult> GetPackAsync(string code)
	{
		var jwt = Request.Cookies["jwt"];
		if (jwt == null)
			return Unauthorized();
		var token = _jwtService.Verify(jwt);
		var isSuccess = int.TryParse(token.Issuer, out var userId);
		if (!isSuccess)
			return BadRequest(new InvalidData("Token"));

		var response = await _databaseHandler.GetPacksByInvestCode(code);

		return response.Status ? Ok(response) : BadRequest(response);
	}
	[HttpPut("reactivate/{id}")]
	public async Task<IActionResult> ReactivatePackAsync(int id)
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
		
		var response = await _databaseHandler.ReactivatePack(id);
		if (!response.Status)
			return BadRequest(response);
		return Ok(response);
	}
	[HttpPost("create")]
	public async Task<IActionResult> CreatePackAsync([FromBody] PackDto packDto)
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
		
		var response = await _databaseHandler.CreatePack(packDto, userId);
		
		if (!response.Status)
			return BadRequest(response);
		
		return Ok(response);
	}

	[HttpGet("nextClose/{investmentCode}")]
	public async Task<IActionResult> GetNextCloseAsync(string investmentCode)
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

		var response = await _databaseHandler.GetPacksByInvestCode(investmentCode);
		if (!response.Status)
			return BadRequest(response);
		var packs = response as OkResponse<List<Pack>>;
		if(packs.ObjectData.Count == 0)
			return NotFound(response);
		return Ok(new OkResponse<Pack>(packs.ObjectData.MinBy(p => p.EndDate)));
	}
	[HttpGet("types")]
	public async Task<IActionResult> GetPackTypes()
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

		var response = await _databaseHandler.GetAllPackTypes();
		if (response == null)
			return BadRequest(new InternalErrorResponse());
		return Ok(new OkResponse<List<PackType>>(response));
	}


}