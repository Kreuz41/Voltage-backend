using FP.Core.Api.Helpers;
using FP.Core.Api.Responses;
using FP.Core.Database.Handlers;
using FP.Core.Database.Models;
using Microsoft.AspNetCore.Mvc;
using Org.BouncyCastle.Asn1.Ocsp;

namespace FP.Core.Api.Controllers;

	[ApiController]
	[Route("api/verify")]
public class VerifyCodeController : ControllerBase
{
	
		private readonly JwtService _jwtService;
		private readonly ILogger<InvestmentController> _logger;
		private readonly VerificationCodeDatabaseHandler _verificationCodeDatabaseHandler;
		private readonly UserDatabaseHandler _userDatabaseHandler;

		public VerifyCodeController(JwtService jwtService, ILogger<InvestmentController> logger,
			VerificationCodeDatabaseHandler verificationCodeDatabaseHandler, UserDatabaseHandler userDatabaseHandler)
		{
			_userDatabaseHandler = userDatabaseHandler;
			_verificationCodeDatabaseHandler = verificationCodeDatabaseHandler;
			_jwtService = jwtService;
			_logger = logger;
		}


	[HttpGet("get/{code}")]
	public async Task<IActionResult> GetUnusedCodes(int code)
	{
		var result = await _verificationCodeDatabaseHandler.FindCode(code);
		if (!result.Status)
			BadRequest(result);
		return Ok((result as OkResponse<VerificationCode>).ObjectData.UserId);
	}

}

