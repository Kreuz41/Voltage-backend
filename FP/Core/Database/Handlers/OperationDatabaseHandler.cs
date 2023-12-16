using FP.Core.Api.Helpers;
using FP.Core.Api.Responses;
using FP.Core.Database.Models;
using Microsoft.EntityFrameworkCore;

namespace FP.Core.Database.Handlers;

public class OperationDatabaseHandler
{
	private readonly ILogger<OperationDatabaseHandler> _logger;
	private readonly FpDbContext _dbContext;
	public OperationDatabaseHandler(FpDbContext dbContext, ILogger<OperationDatabaseHandler> logger)
	{
		_dbContext = dbContext;
		_logger = logger;
	}

	public async Task<ReturnResponse> CreateOperation(int userId, decimal sum, string source, int type, User? user = null)
	{
		ReturnResponse returnResponse;
		try
		{
			Operation operation = new Operation()
			{
				UserId = userId,
				Source = source,
				Partner = user,
				Sum = sum,
				OperationTypeId = type
			};
			_dbContext.Operations.Add(operation);
			await _dbContext.SaveChangesAsync();
			returnResponse = new OkResponse<Operation>(operation);
		}
		catch (Exception ex) 
		{
			returnResponse = new InternalErrorResponse();
		}
		return returnResponse;
	}
	public async Task<ReturnResponse> GetOperationsByUserId(int userId)
	{
		ReturnResponse returnResponse;
		try
		{
			var operations = _dbContext.Operations.Include(o=>o.Partner).Where(o => o.UserId == userId).ToList();
			if (operations != null)
				returnResponse = new OkResponse<List<Operation>>(operations);
			else 
				returnResponse = new OkResponse<List<Operation>>(new List<Operation>());
		}
		catch (Exception ex) 
		{ 
			returnResponse = new InternalErrorResponse(); 
		}
		return returnResponse;
	}
	public async Task<ReturnResponse> GetDealSumFromReferral(int referrerId, int referralId)
	{
		ReturnResponse returnResponse;
		try
		{
			var operations = _dbContext.Operations
				.Where(o => o.UserId == referrerId && o.PartnerId==referralId && o.OperationTypeId == (int)OperationTypeEnum.RefBonus)
				.ToList();
			if (operations != null)
				returnResponse = new OkResponse<decimal>(operations.Sum(o=>o.Sum));
			else
				returnResponse = new OkResponse<decimal>(0);
		}
		catch (Exception ex)
		{
			returnResponse = new InternalErrorResponse();
		}
		return returnResponse;
	}
}
