using FP.Core.Api.ApiDto;
using FP.Core.Api.Responses;
using FP.Core.Database.Models;
using Microsoft.EntityFrameworkCore;

namespace FP.Core.Database.Handlers;

public class TransactionDatabaseHandler
{
    private readonly FpDbContext _dbContext;
    private readonly ILogger<TransactionDatabaseHandler> _logger;
    
    public TransactionDatabaseHandler(FpDbContext dbContext, ILogger<TransactionDatabaseHandler> logger)
    {
        _dbContext = dbContext;
        _logger = logger;
    }
    
    public async Task<ReturnResponse> Create(CreateTransactionDto transferDto, int userId, User receiver)
    {
        try
        {
            Transaction transaction = new Transaction
            {
                FromUserId = userId,
                ToUserId = receiver.Id,
                DealSum = transferDto.Sum,
                ToAgent = transferDto.ToAgent,
                FromAgent = transferDto.FromAgent
            };
            _dbContext.Transactions.Add(transaction);
            await _dbContext.SaveChangesAsync();
            return new OkResponse<Transaction>(transaction);
        }
        catch (Exception ex) 
        {
            return new InternalErrorResponse();
        }
    }

	public async Task<ReturnResponse> ConfirmTransferAsync(int transactionId)
	{
        try
        {
            var transaction = await _dbContext.Transactions.Include(t=> t.FromUser).Include(t=>t.ToUser).FirstOrDefaultAsync(t => t.Id == transactionId);
            transaction.IsConfirmed = true;
            _dbContext.Update(transaction);
            await _dbContext.SaveChangesAsync();
            return new OkResponse<Transaction>(transaction);
        }
        catch 
        { 
            return new InternalErrorResponse(); 
        }
	}
}