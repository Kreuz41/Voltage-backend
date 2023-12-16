using FP.Core.Api.Responses;
using FP.Core.Database.Models;

namespace FP.Core.Database.Handlers;

public class ReferralDatabaseHandler
{
    private readonly FpDbContext _dbContext;
    private readonly UserDatabaseHandler _userDatabaseHandler;
    
    public ReferralDatabaseHandler(FpDbContext dbContext, UserDatabaseHandler userDatabaseHandler)
    {
        _dbContext = dbContext;
        _userDatabaseHandler = userDatabaseHandler;
    }

    public async Task<bool> CreateReferralTree(User user)
    {
        var isSuccess = true;

        try
        {
            var referrer = await _userDatabaseHandler.GetUserByReferralCode(user.ReferrerCode) as OkResponse<User>;
            if(!referrer.Status)
                return false;
          
            for (int i = 0; i < 10; i++)
            {
                Referral referral = new()
                {
                    Ref = user,
                    Referrer = referrer.ObjectData,
                    Inline = i + 1
                };
                await _dbContext.Referrals.AddAsync(referral);
                var responce = await _userDatabaseHandler.GetUserByReferralCode(referrer.ObjectData.ReferrerCode);
                if (!responce.Status)
                {
                    break;
                }
                referrer = responce as OkResponse<User>;
                
            }

            await _dbContext.SaveChangesAsync();
        }
        catch (Exception e)
        {
            isSuccess = false;
        }
        
        return isSuccess;
    }

    public User[] GetReferrersByUserId(int userId)
    {
        try
        {
            var users = _dbContext.Referrals.Where(u => u.RefId == userId).ToArray();
        }
        catch
        {
            return Array.Empty<User>();
        }
        
        return Array.Empty<User>();
    }
}