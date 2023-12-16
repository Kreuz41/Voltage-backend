using FP.Core.Api.ApiDto;
using FP.Core.Api.Helpers;
using FP.Core.Api.Responses;
using FP.Core.Api.Services;
using FP.Core.Database.Handlers;
using FP.Core.Database.Models;
using FP.Core.Database.Models.ResponseDTO;
using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.AspNetCore.Mvc;

namespace FP.Core.Api.Controllers;

[ApiController]
[Route("api/user")]
public class UserController : ControllerBase
{
	private readonly ILogger<UserController> _logger;
	private readonly JwtService _jwtService;
	private readonly UserDatabaseHandler _databaseHandler;
	private readonly ConfirmEmailService _emailService;
	private readonly OperationDatabaseHandler _operationDatabaseHandler;
	private readonly TransactionDatabaseHandler _transactionDatabaseHandler;

	public UserController(UserDatabaseHandler databaseHandler, ILogger<UserController> logger, JwtService jwtService,
		ConfirmEmailService emailService, OperationDatabaseHandler operationDatabaseHandler, TransactionDatabaseHandler transactionDatabaseHandler)
	{
		_databaseHandler = databaseHandler;
		_logger = logger;
		_jwtService = jwtService;
		_emailService = emailService;
		_operationDatabaseHandler = operationDatabaseHandler;
		_transactionDatabaseHandler = transactionDatabaseHandler;
	}


	[HttpPost("create")]
	public async Task<IActionResult> CreateUserAsync([FromBody] UserCreateDto userCreateData)
	{
		var isHaveCode = await _databaseHandler.GetUserByReferralCode(userCreateData.ReferrerCode);

		if (isHaveCode == null)
			BadRequest(new InvalidData("Referral code"));

		var result = await _databaseHandler.CreateUser(userCreateData);

		if (!result.Status)
			return BadRequest(result);

		_logger.LogInformation("User created successfully {result}", result);

		return Ok(result);
	}

	[HttpPost("update")]
	public async Task<IActionResult> UpdateUserAsync([FromBody] UserUpdateDto userUpdateData)
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

		var result = await _databaseHandler.UpdateUser(userId, userUpdateData);

		if (!result.Status)
			return BadRequest(result);

		_logger.LogInformation("User created successfully {result}", result);

		return Ok(result);
	}

	[HttpPost("confirmEmail")]
	public async Task<IActionResult> ConfirmEmailAsync([FromBody] ConfirmEmailDto emailDto)
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

		var user = await _databaseHandler.GetUserById(userId) as OkResponse<User>;
		if (user == null || !user.Status)
			return BadRequest(user);

		_logger.LogInformation("API-Request \n Post | Name=confirm_email");
		var result = await _emailService.SendConfirmEmail(emailDto, user.ObjectData.Email);

		return result ? Ok() : BadRequest("Cannot send confirmation code");
	}

	[HttpGet("confirm/{code}")]
	public async Task<IActionResult> ConfirmAsync(int code)
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

		_logger.LogInformation("API-Request \n Get | Name=confirm/{code}", code);

		var result = await _emailService.ConfirmEmail(code);

		if (!result)
			return BadRequest(new InvalidData("verification code"));

		var confirm = await _databaseHandler.ConfirmEmailAsync(userId);

		return confirm is OkResponse<User> ?
			Ok(confirm) :
			BadRequest(confirm);
	}

	[HttpPost("login")]
	public async Task<IActionResult> LoginUserAsync([FromBody] LoginDto userData)
	{
		_logger.LogInformation("API-Request \n Post | Name=login | {userData}", userData);
		var result = await _databaseHandler.LoginUser(userData);
		if (!result.Status)
			return BadRequest(result);
		var user = ((OkResponse<User>)result).ObjectData;

		if (result is OkResponse<User>)
		{
			var jwtToken = _jwtService.Generate(user.Id);
			Response.Cookies.Append("jwt", jwtToken, new CookieOptions
			{
				HttpOnly = true,
				Secure = true,
				SameSite = SameSiteMode.None
			});

			return Ok(result);
		}

		_logger.LogInformation("Cannot find user {result}", result);
		return BadRequest(result);
	}

	[HttpGet("profile")]
	public async Task<IActionResult> GetUserInfo()
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

		var result = await _databaseHandler.GetUserById(userId);
		return result is OkResponse<User> ? Ok(result) : BadRequest(result);
	}

	[HttpGet("profile/{code}")]
	public async Task<IActionResult> GetUserInfoByCode(string code)
	{

		var jwt = Request.Cookies["jwt"];
		var result = await _databaseHandler.GetUserByReferralCode(code);
		if (!result.Status)
			return BadRequest(result);
		var userData = result as OkResponse<User>;
		if (!userData.ObjectData.ShowEmail)
			userData.ObjectData.Email = string.Empty;
		if (!userData.ObjectData.ShowPhone)
			userData.ObjectData.Phone = string.Empty;
		if (!userData.ObjectData.ShowTg)
			userData.ObjectData.Telegram = string.Empty;
		if (jwt == null)
		{
			return Ok(userData);
		}
		var token = _jwtService.Verify(jwt);
		if (token == null)
			return Ok(result);
		var isSuccess = int.TryParse(token.Issuer, out var userId);
		if (!isSuccess)
			return Ok(result);

		var refInfoResponse = await _databaseHandler.GetReferralInfo(userId, userData.ObjectData.Id);
		if (!refInfoResponse.Status)
			return BadRequest(refInfoResponse);
		var fullRefInfo = refInfoResponse as OkResponse<Referral>;
		var operationsData = await _operationDatabaseHandler.GetDealSumFromReferral(userId, userData.ObjectData.Id);
		if (!operationsData.Status)
			return BadRequest(operationsData);
		var operations = operationsData as OkResponse<decimal>;
		var returnResponse = new ReferralResponseDto
		{
			User = userData.ObjectData,
			Inline = fullRefInfo.ObjectData.Inline,
			DealSum = operations.ObjectData
		};
		return Ok(new OkResponse<ReferralResponseDto>(returnResponse));

	}

	[HttpGet("teams")]
	public async Task<IActionResult> GetTeamsInfo()
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

		var user = await _databaseHandler.GetUserById(userId) as OkResponse<User>;

		if (user == null || !user.Status)
			return BadRequest(user);

		var mainTeam = await _databaseHandler.GetMainTeamGood(userId) as OkResponse<decimal>;
		if (mainTeam == null || !mainTeam.Status)
			return BadRequest(mainTeam);
		var responce = new TeamResponseDto
		{
			Rang = user.ObjectData.Rang,
			Goods = user.ObjectData.LinesIncome,
			MainTeam = mainTeam.ObjectData
		};

		return Ok(new OkResponse<TeamResponseDto>(responce));
	}

	[HttpGet("topUp")]
	public async Task<IActionResult> GetWalletInfo()
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

		return Ok(await _databaseHandler.CheckTopUpWalletBalance(userId));
	}

	[HttpPost("logout")]
	public async Task<IActionResult> LogoutUser()
	{
		Response.Cookies.Delete("jwt");

		return Ok(new OkResponse<User>(null));
	}
	[HttpGet("operations")]
	public async Task<IActionResult> GetAllOperations()
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
		var result = await _operationDatabaseHandler.GetOperationsByUserId(userId);
		return result.Status ? Ok(result) : BadRequest(result);
	}
	[HttpPut("transfer")]
	public async Task<IActionResult> TransferBalance([FromBody] CreateTransactionDto transferDto)
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
		var response = await _databaseHandler.GetUserById(userId);
		if (!response.Status)
			return BadRequest(new NotFoundResult());
		var user = (response as OkResponse<User>).ObjectData;
		if (user == null)
			return BadRequest(new InternalErrorResponse());
		if (user.BalanceAgent < transferDto.Sum)
			return BadRequest(new InvalidData("sum"));
		var receiver = await _databaseHandler.GetUserByReferralCode(transferDto.Code);
		if (!receiver.Status)
		{
			return BadRequest(receiver);
		}
		var transaction = await _transactionDatabaseHandler.Create(transferDto, userId, (receiver as OkResponse<User>).ObjectData);
		var result = await _emailService.SendConfirmEmail(transferDto, user.Email);

		return result ? Ok() : BadRequest("Cannot send confirmation code");
	}
	[HttpPut("confirmTransfer/code={code}&transactionId={transactionId}")]
	public async Task<IActionResult> ConfirmTransfer(int code, int transactionId)
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

		_logger.LogInformation("API-Request \n Get | Name=confirm/{code}", code);

		var result = await _emailService.ConfirmEmail(code);

		if (!result)
			return BadRequest(new InvalidData("verification code"));

		var confirm = await _transactionDatabaseHandler.ConfirmTransferAsync(transactionId);

		if (!confirm.Status)
			return BadRequest(confirm);
		var transaction = (confirm as OkResponse<Transaction>).ObjectData;

		confirm = await _databaseHandler.TransferAsync(transaction);

		return confirm is OkResponse<User> ?
			Ok(confirm) :
			BadRequest(confirm);
	}

	[HttpGet("structure")]
	public async Task<IActionResult> GetStructureAsync()
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

		var result = await _databaseHandler.GetReferralStructure(userId);

		return result.Status ? Ok(result) : BadRequest(result);
	}

	[HttpGet("structure/{line}")]
	public async Task<IActionResult> GetLineInfo(int line)
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

		var result = await _databaseHandler.GetLineFullInfo(userId, line);

		return result.Status ? Ok(result) : BadRequest(result);
	}

	[HttpPost("recover")]
	public async Task<IActionResult> PostRecoveryCode([FromBody] RecoveryDto recoveryDto)
	{
		var response = await _databaseHandler.AddRecoveryCode(recoveryDto);
		if (!response.Status)
			return BadRequest(response);
		var result = await _emailService.SendRecoverEmail(recoveryDto);
		if (!result)
			return BadRequest(new InternalErrorResponse());
		return Ok(response);
	}

	[HttpPut("confirmRecover")]
	public async Task<IActionResult> ConfirmRecoverPassword([FromBody] PasswordRecoveryDto passwordRecoveryDto)
	{
		var response = await _databaseHandler.RecoverPassword(passwordRecoveryDto);
		return response.Status ? Ok(response) : BadRequest(response);
	}
	[HttpPut("verifyTelegram")]
	public async Task<IActionResult> VerifyTelegramUser([FromQuery] int userId, [FromQuery] long chatId)
	{
		var response = await _databaseHandler.VerifyTelegramUser(userId, chatId);
		return response.Status ? Ok(response) : BadRequest(response);
	}
	[HttpPut("changePassword")]
	public async Task<IActionResult> ChangePassword([FromBody] ChangePasswordDto changePasswordDto)
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
		if (changePasswordDto.PrevPassword != changePasswordDto.NewPassword)
			return BadRequest(new InvalidData("Passwords must be same"));
		var response = await _databaseHandler.ChangePassword(userId, changePasswordDto.NewPassword);
		return response.Status ? Ok(response) : BadRequest(response);
	}
}