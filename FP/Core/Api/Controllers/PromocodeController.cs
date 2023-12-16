using FP.Core.Api.ApiDto;
using FP.Core.Api.Helpers;
using FP.Core.Api.Providers.Interfaces;
using FP.Core.Api.Responses;
using FP.Core.Database.Handlers;
using FP.Core.Database.Models;
using Microsoft.AspNetCore.Mvc;
using System.Net.WebSockets;


namespace FP.Core.Api.Controllers
{
	[ApiController]
	[Route("api/promocode")]
	public class PromocodeController : ControllerBase
	{
		private readonly JwtService _jwtService;
		private readonly ILogger<InvestmentController> _logger;
		private readonly PromocodeDatabaseHandler _promocodeDatabaseHandler;
		private readonly UserDatabaseHandler _userDatabaseHandler;

		public PromocodeController(JwtService jwtService, ILogger<InvestmentController> logger,
			PromocodeDatabaseHandler promocodeDatabaseHandler, UserDatabaseHandler userDatabaseHandler)
		{
			_userDatabaseHandler = userDatabaseHandler;
			_promocodeDatabaseHandler = promocodeDatabaseHandler;
			_jwtService = jwtService;
			_logger = logger;
		}

		[HttpPost("create")]
		public async Task<IActionResult> CreatePromocodeAsync([FromBody] int count)
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
			if (await _userDatabaseHandler.GetUserById(userId) is not OkResponse<User> response || !response.Status)
				return BadRequest(new InvalidData("userId"));
			if (!response.ObjectData.IsAdmin)
				return BadRequest(new InvalidDataException("Access denied"));

            await _promocodeDatabaseHandler.CreatePromocodes(count);

			return Ok();
		}
		[HttpGet("unused")]
		public async Task<IActionResult> GetUnusedCodes()
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
			if (await _userDatabaseHandler.GetUserById(userId) is not OkResponse<User> response || !response.Status)
				return BadRequest(new InvalidData("userId"));
			if (!response.ObjectData.IsAdmin)
				return Forbid();

            var result  = await _promocodeDatabaseHandler.GetInactiveCodes();
			return result.Status ? Ok(result) : BadRequest(result);
		}


	}
}
