using FP.Core.Api.Helpers;
using FP.Core.Database;
using FP.Core.Database.Handlers;
using FP.Core.Database.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.Net.Http.Headers;

namespace FP.Core.Api.Services
{
	public class PackAccrualSchedulerService : BackgroundService
	{
		private readonly IDbContextFactory<FpDbContext> _dbContextFactory;
		private readonly ILogger<PackAccrualSchedulerService> _logger;

		public PackAccrualSchedulerService(IDbContextFactory<FpDbContext> dbContextFactory, ILogger<PackAccrualSchedulerService> logger)
		{
			_dbContextFactory = dbContextFactory;
			_logger = logger;
		}
		
		protected override async Task ExecuteAsync(CancellationToken stoppingToken)
		{
			_logger.LogInformation("Timed Hosted Service running.");

			using PeriodicTimer timer = new(TimeSpan.FromDays(1));

			try
			{
				while (await timer.WaitForNextTickAsync(stoppingToken))
				{
					await Task.Run(async () =>
					{
						var investments = await InvestmentsAccrualAsync();
						foreach (var investment in investments)
						{
							await PacksAccrualAsync(investment);
						}
					}, stoppingToken);
				}
			}
			catch (OperationCanceledException)
			{
				_logger.LogInformation("Timed Hosted Service is stopping.");
			}
		}

		private async Task<Investment[]> InvestmentsAccrualAsync()
		{
			try
			{
				await using var context = await _dbContextFactory.CreateDbContextAsync();
				var investment = context.Investments
					.Include(i=>i.Promocode)
					.Where(i => !i.IsEnded).ToArray();
				_logger.LogInformation("Investments found {0}", investment.Length);
			
				return investment;
			}
			catch
			{
				return Array.Empty<Investment>();
			}
		}
		
		private async Task PacksAccrualAsync(Investment investment)
		{
			try
			{
				await using var context = await _dbContextFactory.CreateDbContextAsync();
				var packs = context.Packs.Where(p => p.InvestmentId == investment.Id).ToArray();
				var user = await context.Users.FirstOrDefaultAsync(u => u.Id == investment.UserId);
				
				if(user == null)
					return;

				
				var sum = investment.TotalSum * investment.TotalYield / 100m * 0.77m;

				await TopUpIncome(context, user, sum, investment);
				var referrerals = context.Referrals.Where(u => u.RefId == user.Id).ToArray();
				var refsId = referrerals.Select(r => r.ReferrerId).ToArray();
				var referrers = context.Users.Where(u => refsId.Contains(u.Id)).ToArray();
				
				foreach (var t in referrers)
				{
					var inline = referrerals.FirstOrDefault(r => r.ReferrerId == t.Id).Inline;
					await TopUpAgent(context, t, (sum * DefineIncome(t.Rang, inline)), investment.Promocode.Code, user.Id);
				}
				
				foreach (var pack in packs)
				{
					if (pack.HasLastAccrual)
						continue;

					if (pack.EndDate < DateTime.UtcNow)
					{
						user.BalanceIncome += pack.DealSum;
						user.CurrentIncome -= pack.DealSum;
						pack.HasLastAccrual = true;
					}
					
					investment.TotalYield = packs.Where(p => !p.HasLastAccrual).Sum(p => p.Yield);
				}

				if(packs.Length > 0)
					investment.IsEnded = packs.All(p => p.HasLastAccrual);

				if (investment.IsEnded)
				{
					var rangHelper = new UpdateRang(context);
					await rangHelper.UpdateLineIncome(-packs.Sum(p => p.DealSum), user.ReferrerCode);
				}
				
				context.Users.Update(user);
				context.Packs.UpdateRange(packs);
				context.Users.UpdateRange(referrers);
				context.Investments.Update(investment);
				
				await context.SaveChangesAsync();
			}
			catch (Exception e)
			{
				Console.WriteLine(e);
			}
		}

		private decimal DefineIncome(int rang, int inline)
		{
			switch (inline)
			{
				case 1:
					if (rang >= 1)
						return 0.05m;
					break;
				case 2:
					if (rang >= 2)
						return 0.04m;
					break;
				case 3:
					if (rang >= 3)
						return 0.035m;
					break;
				case 4:
					if (rang >= 4)
						return 0.03m;
					break;
				case 5:
					if (rang >= 6)
						return 0.025m;
					break;
				case 6:
					if (rang >= 8)
						return 0.02m;
					break;
				case 7:
					if (rang >= 10)
						return 0.015m;
					break;
				case 8:
					if (rang >= 12)
						return 0.01m;
					break;
				case 9:
					if (rang >= 14)
						return 0.005m;
					break;
				case 10:
					if (rang == 15)
						return 0.003m;
					break;
			}

			return 0m;
		}
		public async Task TopUpAgent(FpDbContext context, User user, decimal sum, string code, int partnerId)
		{
			try
			{
				user.BalanceAgent += sum;
				user.TotalIncome += sum;
				Operation operation = new Operation()
				{
					IsAgentBalance = true,
					Sum = sum,
					UserId = user.Id,
					OperationTypeId = (int)OperationTypeEnum.RefBonus,
					Source = code,
					PartnerId = partnerId
				};
				context.Users.Update(user);
				context.Add(operation);
				await context.SaveChangesAsync();
			}
			catch (Exception ex) { }
		}
		public async Task TopUpIncome(FpDbContext context, User user, decimal sum, Investment invest)
		{
			try
			{
				user.BalanceIncome += sum;
				user.TotalIncome += sum;
				Operation operation = new Operation()
				{
					UserId = user.Id,
					IsAgentBalance = false,
					Sum = sum,
					Source = invest.Promocode.Code,
					PartnerId = (await context.Users.FirstOrDefaultAsync(u => u.ReferralCode == user.ReferrerCode)).Id,
					OperationTypeId = (int)OperationTypeEnum.Accrual,
				};
				invest.TotalAccrual += sum;
				context.Investments.Update(invest);
				context.Users.Update(user);
				context.Add(operation);
				await context.SaveChangesAsync();
			}
			catch (Exception ex) { }
		}
	}
}
