using FP.Core.Api.Helpers;
using FP.Core.Api.Responses;
using FP.Core.Database.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace FP.Core.Database.Handlers
{
	public class PromocodeDatabaseHandler
	{
		private readonly ILogger<InvestmentDatabaseHandler> _logger;
		private readonly FpDbContext _dbContext;
		public PromocodeDatabaseHandler(IDbContextFactory<FpDbContext> dbContext, ILogger<InvestmentDatabaseHandler> logger)
		{
			_dbContext = dbContext.CreateDbContext();
			_logger = logger;
		}

		public async Task CreatePromocodes(int count)
		{
			_logger.LogInformation("Start to create {count} promocodes", count);
			try
			{
				RandomStringBuilder randomStringBuilder = new();
				Promocode[] codes = new Promocode[count];
				for (int i = 0; i < count; i++)
				{
					codes[i] = new Promocode()
					{
						Code = randomStringBuilder.Create(10),
					};
				}
				
				await _dbContext.PromoCodes.AddRangeAsync(codes);
				await _dbContext.SaveChangesAsync();
			}
			catch (Exception ex) 
			{ 
				_logger.LogError(ex.Message);
			}
		}
		public async Task<int> GetPromoCodeId(string code)
		{
			try
			{
				var promocode = await _dbContext.PromoCodes.FirstOrDefaultAsync(x => x.Code == code);
                if (promocode.IsActivated)
					return 0;
                return promocode.Id;
			}
			catch(Exception ex) 
			{
				_logger.LogError(ex.Message);
				return 0;
			}
		}
		public async Task ActivatePromocode(int id)
		{
			try
			{
				var promocode = await _dbContext.PromoCodes.FirstOrDefaultAsync(x => x.Id == id);
				promocode.IsActivated = true;
				_dbContext.Update(promocode);
				await _dbContext.SaveChangesAsync();

			}
			catch (Exception ex)
			{
				_logger.LogError(ex.Message);
			}
		}
		public async Task<ReturnResponse> GetInactiveCodes()
		{
			try
			{
				return new OkResponse<List<Promocode>>(_dbContext.PromoCodes.ToList());
			}
			catch (Exception ex)
			{
				return new InternalErrorResponse();
			}
		}
	}
}

