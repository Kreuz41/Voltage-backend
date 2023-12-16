using FP.Core.Api.Responses;
using FP.Core.Database.Models;
using Microsoft.EntityFrameworkCore;

namespace FP.Core.Database.Handlers;

public class VerificationCodeDatabaseHandler
{
	private readonly FpDbContext _dbContext;
	private readonly ILogger<VerificationCodeDatabaseHandler> _logger;
    
    public VerificationCodeDatabaseHandler(FpDbContext dbContext, ILogger<VerificationCodeDatabaseHandler> logger)
    {
		_dbContext = dbContext;
        _logger = logger;
    }

    public async Task<bool> Create(int code, int? userId = null)
    {
        var isSuccess = true;
        _logger.LogInformation("Start to create new verification code");

        try
        {

            var res = await _dbContext.VerificationCodes.FirstOrDefaultAsync(v => v.Code == code);

            if (res != null)
            {
                res.IsActive = true;
                await _dbContext.SaveChangesAsync();
                return isSuccess;
            }
            
            var verCode = new VerificationCode
            {
                Code = code,
                UserId = userId
            };

            await _dbContext.VerificationCodes.AddAsync(verCode);
            await _dbContext.SaveChangesAsync();
        }
        catch (Exception e)
        {
            _logger.LogError(e.Message);
            isSuccess = false;
        }
        
        return isSuccess;
    }

    public async Task<ReturnResponse> FindCode(int code)
    {
        try
        {
            
            var result = await _dbContext.VerificationCodes.Include(c=> c.User).FirstOrDefaultAsync(v => v.Code == code);
            
            if (result == null)
                return new NotFoundResponse();

            if (result.IsActive)
            {
                result.IsActive = false;
                _dbContext.Update(result);
                await _dbContext.SaveChangesAsync();
                return new OkResponse<VerificationCode>(result);
            }
        }
        catch (Exception e)
        {
            _logger.LogError(e.Message);
        }
        
        return new InternalErrorResponse();
    }
}