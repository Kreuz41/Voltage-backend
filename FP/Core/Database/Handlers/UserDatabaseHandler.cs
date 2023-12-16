using FP.Core.Api.ApiDto;
using FP.Core.Api.Providers.Interfaces;
using FP.Core.Database.Models;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using FP.Core.Api.Helpers;
using FP.Core.Api.Responses;
using FP.Core.Database.Models.ResponseDTO;


namespace FP.Core.Database.Handlers;

public class UserDatabaseHandler
{
	private readonly ILogger<UserDatabaseHandler> _logger;
	private readonly FpDbContext _dbContext;
	private readonly IServiceProvider _serviceProvider;
	private readonly WalletDatabaseHandler _walletDatabaseHandler;
	private readonly ICryptoApiTrc20Provider _cryptoApiProvider;
	private readonly ReferralDatabaseHandler _referralDatabaseHandler;
	private readonly OperationDatabaseHandler _operationDatabaseHandler;
	private readonly InvestmentDatabaseHandler _investmentDatabaseHandler;
	private readonly VerificationCodeDatabaseHandler _verificationCodeDatabaseHandler;

	public UserDatabaseHandler(FpDbContext dbContext, WalletDatabaseHandler walletDatabaseHandler, IServiceProvider service,
		ILogger<UserDatabaseHandler> logger, ICryptoApiTrc20Provider cryptoApiProvider, 
		OperationDatabaseHandler operationDatabaseHandler, InvestmentDatabaseHandler investmentDatabaseHandler,
		VerificationCodeDatabaseHandler verificationCodeDatabaseHandler)
	{
		_dbContext = dbContext;
		_serviceProvider = service;
		_logger = logger;
		_walletDatabaseHandler = walletDatabaseHandler;
		_cryptoApiProvider = cryptoApiProvider;
		_referralDatabaseHandler = new ReferralDatabaseHandler(dbContext, this);
		_operationDatabaseHandler = operationDatabaseHandler;
		_investmentDatabaseHandler = investmentDatabaseHandler;
		_verificationCodeDatabaseHandler = verificationCodeDatabaseHandler;
	}

	public async Task<ReturnResponse> CreateUser(UserCreateDto userCreateData)
	{
		_logger.LogInformation($"Start to add user in database {userCreateData}");

		ReturnResponse response;
		var hasher = _serviceProvider.GetRequiredService<IPasswordHasher<User>>();
		RandomStringBuilder stringBuilder = new();
		User user = new()
		{
			Email = userCreateData.Email,
			TopUpWallet = await _walletDatabaseHandler.CreateWallet(),
			ReferrerCode = userCreateData.ReferrerCode,
			ReferralCode = stringBuilder.Create(7),
			BalanceIncome = 0m,
			BalanceCrypto = 0m,
			BalanceAgent = 0m,
		};
		user.Passwordhash = hasher.HashPassword(user, userCreateData.Password);

		try
		{
			var result = await _dbContext.Users.AnyAsync(u => u.Email == user.Email);
			if (!result)
			{
				var addedUser = await _dbContext.Users.AddAsync(user);
				await _dbContext.SaveChangesAsync();
				var res = await _referralDatabaseHandler.CreateReferralTree(addedUser.Entity);
				if (!res)
					return new InvalidData("referral tree");
				_logger.LogInformation("User created");
				response = new OkResponse<User>(addedUser.Entity);
			}
			else
			{
				_logger.LogInformation("Cannot create user with email {Email}", user.Email);
				response = new InvalidData("email");
			}
		}
		catch (Exception ex)
		{
			response = new InternalErrorResponse();
			_logger.LogInformation(ex, "Cannot create user");
		}
		return response;
	}

	public async Task<ReturnResponse> UpdateUser(int userId, UserUpdateDto data)
	{
		try
		{
			var user = await _dbContext.Users.FirstOrDefaultAsync(u => u.Id == userId);
			
			if (user == null)
				return new InvalidData("userId");

			user.Avatar = data.Avatar;
			user.Name = data.Name;
			user.Surname = data.Surname;
			user.Nickname = data.Nickname;
			user.Country = data.Country;
			user.City = data.City;
			user.Phone = data.Phone;
			user.Telegram = data.Telegram;

			user.ShowEmail = data.ShowEmail;
			user.ShowTg = data.ShowTg;
			user.ShowPhone = data.ShowPhone;

			if (!user.IsRegistrationEnded)
				user.IsRegistrationEnded = true;

			_dbContext.Update(user);
			await _dbContext.SaveChangesAsync();

			return new OkResponse<User>(user);
		}
		catch (Exception e)
		{
			return new InternalErrorResponse();
		}
	}
	
	public async Task<ReturnResponse> WithdrawInternal(int userId, decimal sum)
	{
		try
		{
			var user = await _dbContext.Users.FirstOrDefaultAsync(u => u.Id == userId);
			if (user == null || user.BalanceIncome < sum)
				return new InvalidData("userId or sum");
			user.BalanceIncome -= sum;
			_dbContext.Update(user);
			await _dbContext.SaveChangesAsync();
			return new OkResponse<User>(user);
		}
		catch 
		{
			return new InternalErrorResponse();
		}
	}
	public async Task<ReturnResponse> WithdrawAgent(int userId, decimal sum)
	{
		try
		{
			var user = await _dbContext.Users.FirstOrDefaultAsync(u => u.Id == userId);
			if (user == null || user.BalanceAgent < sum)
				return new InvalidData("userId or sum");
			user.BalanceAgent -= sum;
			_dbContext.Update(user);
			await _dbContext.SaveChangesAsync();
			return new OkResponse<User>(user);
		}
		catch 
		{
			return new InternalErrorResponse();
		}
	}

	public async Task<ReturnResponse> GetUserByReferralCode(string code)
	{
		try
		{
			var user = await _dbContext.Users.FirstOrDefaultAsync(u => u.ReferralCode == code);
			return user == null ? new NotFoundResponse() : new OkResponse<User>(user);
		}
		catch (Exception e)
		{
			return new InternalErrorResponse();
		}
	}

	public async Task<ReturnResponse> GetMainTeamGood(int userId)
	{
		try
		{
			var refs = _dbContext.Referrals
				.Include(r => r.Ref)
				.Where(r => r.ReferrerId == userId && r.Inline == 1).ToList();
			if (refs.Count == 0)
				return new OkResponse<decimal>(0);
			var maxLineIncome = refs.Max(r => r.Ref.LinesIncome + r.Ref.CurrentIncome);

			return new OkResponse<decimal>(maxLineIncome);
		}
		catch (Exception e)
		{
			return new InternalErrorResponse();
		}
	}
	
	public async Task<ReturnResponse> LoginUser(LoginDto userData)
	{
		_logger.LogInformation("Start to find user in database {userData}", userData);

		ReturnResponse response;
		var hasher = _serviceProvider.GetRequiredService<IPasswordHasher<User>>();
		try
		{
			var result = await _dbContext.Users.FirstOrDefaultAsync(u => u.Email == userData.Email);
			if (result != null)
			{
				if (hasher.VerifyHashedPassword(result, result.Passwordhash, userData.Password) == PasswordVerificationResult.Success)
				{
					_logger.LogInformation("User found");
					result.LastActivityTime = DateTime.UtcNow;
					_dbContext.Update(result);
					await _dbContext.SaveChangesAsync();
					response = new OkResponse<User>(result);
				}
				else
				{
					response = new InvalidData("password");
				}
			}
			else
			{
				response = new InvalidData("email");
			}
		}
		catch (Exception ex)
		{
			_logger.LogInformation(ex, "Cannot create user");
			response = new InternalErrorResponse();
		}
		return response;

	}
	public async Task<ReturnResponse> GetUserWalletById(int userId)
	{
		try
		{
			_logger.LogInformation("Start to find user in database {userId}", userId);
			var user = await _dbContext.Users.Include(u => u.TopUpWallet).FirstOrDefaultAsync(u => userId == u.Id);
			if (user == null) 
			{ 
				return new NotFoundResponse(); 
			}
			return new OkResponse<Wallet>(user.TopUpWallet);
		}
		catch
		{
			return new InternalErrorResponse();
		}
	}

	public async Task<ReturnResponse> ConfirmEmailAsync(int userId)
	{
		try
		{
			var user = await _dbContext.Users.FirstOrDefaultAsync(u => u.Id == userId);
			
			if (user == null)
				return new InvalidData("userId");
			
			user.IsVerified = true;
			_dbContext.Update(user);
			await _dbContext.SaveChangesAsync();
			return new OkResponse<User>(user);
		}
		catch (Exception e)
		{
			return new InternalErrorResponse();
		}
	}

	public async Task<ReturnResponse> GetUserById(int userId)
	{
		try
		{
			var user = await _dbContext.Users
				.Include(u => u.Investments)
				.Include(u => u.Packs)
				.Include(u => u.TopUpWallet)
				.FirstOrDefaultAsync(u => u.Id == userId);

			return user == null ? new InvalidData("UserId") : new OkResponse<User>(user);
		}
		catch (Exception e)
		{
			_logger.LogInformation(e, "Cannot find user");

			return new InternalErrorResponse();
		}
	}
	public async Task<ReturnResponse> UserTopUpInternal(int userId, decimal amount)
	{
		try
		{
			var user = await _dbContext.Users.FirstOrDefaultAsync<User>(u => u.Id == userId);
			user.BalanceIncome += amount;
			_dbContext.Users.Update(user);
			await _dbContext.SaveChangesAsync();
			return new OkResponse<User>(user);
		}
		catch (Exception e) 
		{
			return new InternalErrorResponse(); 
		}
	}
	public async Task<ReturnResponse> UserTopUpAgent(int userId, decimal amount)
	{
		try
		{
			var user = await _dbContext.Users.FirstOrDefaultAsync<User>(u => u.Id == userId);
			user.BalanceAgent += amount;
			_dbContext.Users.Update(user);
			await _dbContext.SaveChangesAsync();
			return new OkResponse<User>(user);
		}
		catch (Exception e) 
		{
			return new InternalErrorResponse(); 
		}
	}

	public async Task<decimal?> CheckTopUpWalletBalance(int userId)
	{
		try
		{
			var user = await _dbContext.Users
				.Include(u => u.Investments)
				.Include(u => u.Packs)
				.Include(user => user.TopUpWallet)
				.FirstOrDefaultAsync(u => u.Id == userId);
			
				var balance = await _cryptoApiProvider.GetTrc20WalletBalance((await _dbContext.Wallets.FirstOrDefaultAsync(w => w.Id == user.TopUpWalletId)).WalletAddress);
				var res = await _cryptoApiProvider.TransferTrc20(user.TopUpWallet, _dbContext.Users.First().TopUpWallet, balance.Value);
				
				if (!res) 
					return null;
				
				user.BalanceIncome += balance.Value;
				_dbContext.Users.Update(user);

				return balance;
			 
		}
		catch (Exception e)
		{
			return null;
		}
	}
	
	public async Task<ReturnResponse> TransferAsync(Transaction transaction)
	{
		ReturnResponse response;
		try
		{
			if (!transaction.FromAgent)
			{
				response = await WithdrawInternal(transaction.FromUserId, transaction.DealSum);
				if (response.Status)
				{
					response = await UserTopUpInternal(transaction.ToUserId, transaction.DealSum);
					await _operationDatabaseHandler.CreateOperation(transaction.FromUserId, transaction.DealSum,
						"Transfer", (int)OperationTypeEnum.Withdraw, transaction.ToUser);
					await _operationDatabaseHandler.CreateOperation(transaction.ToUserId, transaction.DealSum,
						"Transfer", (int)OperationTypeEnum.Accrual, transaction.FromUser);
					return response;
				}
			}
			else
			{
				if (transaction.ToAgent)
				{
					response = await WithdrawAgent(transaction.FromUserId, transaction.DealSum);
					if (response.Status)
						response = await UserTopUpAgent(transaction.ToUserId, transaction.DealSum);
				}
				else
				{
					response = await WithdrawAgent(transaction.FromUserId, transaction.DealSum);
					if (response.Status)
						response = await UserTopUpInternal(transaction.ToUserId, transaction.DealSum);
				}

				await _operationDatabaseHandler.CreateOperation(transaction.FromUserId, transaction.DealSum, "Transfer",
					(int)OperationTypeEnum.Withdraw, transaction.ToUser);
				await _operationDatabaseHandler.CreateOperation(transaction.ToUserId, transaction.DealSum, "Transfer",
					(int)OperationTypeEnum.Accrual, transaction.FromUser);
				return response;
			}

			return new InvalidData("fromInternal to Agent");
		}
		catch
		{
			return new InternalErrorResponse();
		}
	}

	public async Task<ReturnResponse> GetReferralStructure(int userId)
	{
		var infos = new List<ReferralTreeResponseDto>();
		for (var i = 0; i < 11; i++)
		{
			var result = await GetReferralLineInfo(userId, i + 1);
			
			if(result.Status)
				infos.Add(((OkResponse<ReferralTreeResponseDto>)result).ObjectData);
			else
				break;
		}

		return  new OkResponse<List<ReferralTreeResponseDto>>(infos);
	}

	public async Task<ReturnResponse> GetLineFullInfo(int userId, int line)
	{
		try
		{
			var referrals = _dbContext.Referrals
				.Include(u => u.Referrer)
				.Include(referral => referral.Ref)
				.Where(r => r.ReferrerId == userId && r.Inline == line)
				.ToList();

			if (referrals.Count() == 0)
				return new NotFoundResponse();
			
			var users = referrals.Select(u => u.Ref);
			var usersInLine = new List<ReferralLineResponseDto>();

			foreach (var user in users)
			{
				var response = new ReferralLineResponseDto();
				var userInvestment = await _investmentDatabaseHandler.GetAllInvestments(user.Id);

				if (userInvestment.Status)
				{
					response.ActivePacks = (userInvestment as OkResponse<List<Investment>>).ObjectData.Sum(i => i.TotalSum);
				}
				
				var operations = _dbContext.Operations
					.Where(o => o.PartnerId == user.Id && o.UserId == userId && o.OperationTypeId  == (int)OperationTypeEnum.RefBonus)
					.ToList();

				
				response.MyIncome = operations.Sum(o => o.Sum);
				response.Income = user.TotalIncome;
				response.UsersCount = _dbContext.Referrals.Count(r => r.ReferrerId == user.Id);
				response.User = user;
				
				usersInLine.Add(response);
			}

			return new OkResponse<List<ReferralLineResponseDto>>(usersInLine);
		}
		catch (Exception e)
		{
			return new InternalErrorResponse();
		}
	}

	public async Task<ReturnResponse> GetReferralLineInfo(int userId, int line)
	{
		try
		{
			var referrals = _dbContext.Referrals
				.Include(u => u.Referrer)
				.Include(referral => referral.Ref)
				.Where(r => r.ReferrerId == userId && r.Inline == line)
				.ToList();

			if (referrals.Count() == 0)
				return new NotFoundResponse();
			
			var users = referrals.Select(u => u.Ref);
			var response = new ReferralTreeResponseDto();

			foreach (var user in users)
			{
				var userInvestment = await _investmentDatabaseHandler.GetAllInvestments(user.Id);

				if (userInvestment.Status)
				{
					response.ActivePacks += (userInvestment as OkResponse<List<Investment>>).ObjectData.Sum(i => i.TotalSum);
				}
				
				var operations = _dbContext.Operations
					.Where(o => o.PartnerId == user.Id && o.UserId == userId && o.OperationTypeId  == (int)OperationTypeEnum.RefBonus)
					.ToList();
				
				response.MyIncome += operations.Sum(o => o.Sum);
				response.Income += user.TotalIncome;
			}

			response.UsersCount = users.Count();

			return new OkResponse<ReferralTreeResponseDto>(response);
		}
		catch (Exception e)
		{
			return new InvalidData("userId");
		}
	}
	public async Task<ReturnResponse> AddRecoveryCode(RecoveryDto recoveryDto)
	{
		try
		{
			var user = await _dbContext.Users.FirstOrDefaultAsync(u => u.Email == recoveryDto.Email);
			if (user == null)
				return new NotFoundResponse();
			var code = recoveryDto.Url.Split('=')[1];
			await _verificationCodeDatabaseHandler.Create(Convert.ToInt32(code), user.Id);
			return new OkResponse<User>(user);
		}
		catch 
		{
			return new InternalErrorResponse(); 
		}
		
	}
	public async Task<ReturnResponse> RecoverPassword(PasswordRecoveryDto passwordRecoveryDto)
	{
		try
		{
			var hasher = _serviceProvider.GetRequiredService<IPasswordHasher<User>>();
			var result = await _verificationCodeDatabaseHandler.FindCode(Convert.ToInt32(passwordRecoveryDto.Code));
			if(!result.Status)
				return new InvalidData("Code");
			var user = await _dbContext.Users.FirstOrDefaultAsync(u => u.Email == passwordRecoveryDto.Email);
			if(user == null)
				return new NotFoundResponse();
			user.Passwordhash = hasher.HashPassword(user, passwordRecoveryDto.Password);
			_dbContext.Update(user);
			await _dbContext.SaveChangesAsync();
			return new OkResponse<User>(user);
		}
		catch
		{
			return new InternalErrorResponse();
		}
	}
	public async Task<ReturnResponse> VerifyTelegramUser(int userId, long chatId)
	{
		try
		{
			var user = await _dbContext.Users.FirstOrDefaultAsync(u => u.Id == userId);
			if (user == null)
				return new NotFoundResponse();
			user.IsVerified = true;
			user.TelegramId = chatId;
			_dbContext.Update(user);
			await _dbContext.SaveChangesAsync();
			return new OkResponse<User>(user);
		}
		catch
		{
			return new InternalErrorResponse();
		}
	}
	public async Task<ReturnResponse> ChangePassword(int userId, string password)
	{
		var hasher = _serviceProvider.GetRequiredService<IPasswordHasher<User>>();
		RandomStringBuilder stringBuilder = new();
		User user = await _dbContext.Users.FirstOrDefaultAsync(u => u.Id == userId);
		if (user == null)
			return new NotFoundResponse();
		user.Passwordhash = hasher.HashPassword(user, password);
		_dbContext.Update(user);
		await _dbContext.SaveChangesAsync();
		return new OkResponse<User>(user);
	}
	public async Task<ReturnResponse> GetReferralInfo(int referrerId, int referralId)
	{
		var refInfo = _dbContext.Referrals
			.Include(r=>r.Ref)
			.Include(r=>r.Referrer)
			.Where(r=> r.ReferrerId==referrerId && r.RefId==referralId)
			.FirstOrDefault();
		if (refInfo == null)
			return new NotFoundResponse();
		return new OkResponse<Referral>(refInfo);
	}
}
