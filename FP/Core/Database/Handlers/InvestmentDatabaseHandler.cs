using FP.Core.Api.Helpers;
using FP.Core.Api.Responses;
using FP.Core.Database.Models;
using Microsoft.EntityFrameworkCore;


namespace FP.Core.Database.Handlers;

public class InvestmentDatabaseHandler
{
	private readonly ILogger<InvestmentDatabaseHandler> _logger;
	private readonly FpDbContext _dbContext;
	private readonly PromocodeDatabaseHandler _promocodeDatabaseHandler;
	private readonly OperationDatabaseHandler _operationDatabaseHandler;
	public InvestmentDatabaseHandler(FpDbContext dbContext, ILogger<InvestmentDatabaseHandler> logger,
		PromocodeDatabaseHandler promocodeDatabaseHandler, OperationDatabaseHandler operationDatabaseHandler)
	{
		_dbContext = dbContext;
		_logger = logger;
		_promocodeDatabaseHandler = promocodeDatabaseHandler;
		_operationDatabaseHandler = operationDatabaseHandler;
	}

	public async Task<ReturnResponse> CreateInvestment(int userId, string code)
	{
		_logger.LogInformation("Start to add pack in database end date: {endDate}");
		
		try
		{
			var user = await _dbContext.Users.FirstOrDefaultAsync(u => u.Id == userId);

			if (user == null)
				return new InvalidData("userId");
			
			if (!user.IsRegistrationEnded || !user.IsVerified)
				return new NotVerified();
			
			var codeId = await _promocodeDatabaseHandler.GetPromoCodeId(code);
			if (codeId == 0)
				return new InvalidData(code);
			
			Investment investment = new()
			{
				UserId = userId,
				CodeId = codeId,
				TotalSum = 0,
				TotalYield = 0,
			};
			var addedInvestment = await _dbContext.Investments.AddAsync(investment);
			await _dbContext.SaveChangesAsync();
			await _promocodeDatabaseHandler.ActivatePromocode(codeId);
		
			
			_logger.LogInformation("Investment created");
			return new OkResponse<Investment>(addedInvestment.Entity);
		}
		catch (Exception ex)
		{
			_logger.LogInformation("Cannot create pack");
			return new InternalErrorResponse();
		}

	}
	
	public async Task<ReturnResponse> AddPackToInvestment(string code, Pack pack, int userId)
	{
		try
		{
			var invest = _dbContext.Investments
				.Include(i => i.Promocode)
				.Include(i => i.User)
				.Include(i => i.Packs)
				.FirstOrDefault(x => x.Promocode.Code == code);

                if (invest == null || invest.IsEnded)
				return new NotFoundResponse();
                
			if (invest.User == null || invest.User.Id != userId)
				return new InvalidData("userId");
			
			if (!invest.User.IsRegistrationEnded || !invest.User.IsVerified)
				return new NotVerified();

			if (invest.Packs.Count >= 5)
				return new InvalidData("Too many packs");

			pack.InvestmentId = invest.Id;
                
			invest.TotalSum += pack.DealSum;
			invest.MaxSum += pack.DealSum;
			invest.TotalYield += pack.Yield;
			invest.PacksCount++;
			invest.EndDate = invest.EndDate > pack.EndDate ?
				invest.EndDate :
				pack.EndDate;
			_dbContext.Update(invest);
			await _dbContext.Packs.AddAsync(pack);
			await _dbContext.SaveChangesAsync();
			var response = await _operationDatabaseHandler.CreateOperation(userId, pack.DealSum, invest.Promocode.Code, (int)OperationTypeEnum.AssetsCreate);
			return new OkResponse<Investment>(invest);
		}
		catch 
		{
			return new InternalErrorResponse();
		}
	}
	
	public async Task<ReturnResponse> GetAllInvestments(int userId)
	{
		try
		{
			var invests = _dbContext.Investments.Include(i=>i.Promocode).Where(i => i.UserId == userId).ToList();
			
			return new OkResponse<List<Investment>>(invests);
		}
		catch 
		{
			return new InternalErrorResponse();
		}
	}
}