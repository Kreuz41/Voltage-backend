using FP.Core.Api.ApiDto;
using FP.Core.Api.Controllers;
using FP.Core.Api.Helpers;
using FP.Core.Api.Responses;
using FP.Core.Database.Models;
using Microsoft.EntityFrameworkCore;
using Org.BouncyCastle.Utilities.Collections;

namespace FP.Core.Database.Handlers
{
	public class PackDatabaseHandler
	{
		private readonly ILogger<UserController> _logger;
		private readonly FpDbContext _dbContext;
		private readonly InvestmentDatabaseHandler _investmentDatabaseHandler;
		private readonly UserDatabaseHandler _userDatabaseHandler;

		public PackDatabaseHandler(FpDbContext dbContext, ILogger<UserController> logger,
			InvestmentDatabaseHandler investmentDatabaseHandler, UserDatabaseHandler userDatabaseHandler)
		{
			_dbContext = dbContext;
			_logger = logger;
			_investmentDatabaseHandler = investmentDatabaseHandler;
			_userDatabaseHandler = userDatabaseHandler;
		}

		public async Task<ReturnResponse> CreatePack(PackDto packDto, int userId)
		{
			_logger.LogInformation("Start to add pack in database end date: {}", packDto.EndDate);


			ReturnResponse response;
			try
			{
				var packType = await _dbContext.PackTypes.FirstOrDefaultAsync(p => p.Id == packDto.PackTypeId);
				if (packType == null) return new InvalidData("PackTypeId");

				Pack pack = new()
				{
					EndDate = packDto.EndDate,
					DealSum = packDto.DealSum,
					PackTypeId = packDto.PackTypeId,
					Yield = packType.Yeild
				};

				var user = await _userDatabaseHandler.GetUserById(userId) as OkResponse<User>;
				if (user == null || !user.Status)
					return new InvalidData("userId");

				var res = await _investmentDatabaseHandler.AddPackToInvestment(packDto.InvestmentCode, pack, userId);

				if (res is not OkResponse<Investment>)
					return new InvalidData("Investment code");

				user.ObjectData.CurrentIncome += packDto.DealSum;
				_dbContext.Update(user.ObjectData);
				await _dbContext.SaveChangesAsync();

				var userRangHelper = new UpdateRang(_dbContext);
				await userRangHelper.UpdateLineIncome(pack.DealSum, user.ObjectData.ReferrerCode);

				_logger.LogInformation("Pack created");
				response = new OkResponse<Pack>(pack);
			}
			catch
			{
				_logger.LogInformation("Cannot create pack");
				response = new InternalErrorResponse();
			}

			return response;
		}
		public async Task<ReturnResponse> ReactivatePack(int packId)
		{
			ReturnResponse response;
			try
			{
				var pack = await _dbContext.Packs.Include(p => p.Investment).FirstOrDefaultAsync(x => x.Id == packId);
				if (pack == null)
					return new NotFoundResponse();
				pack.EndDate += pack.EndDate - pack.StartDate;
				pack.Investment.EndDate = pack.EndDate;
				_dbContext.Update(pack);
				await _dbContext.SaveChangesAsync();
				response = new OkResponse<Pack>(pack);
			}
			catch
			{
				response = new InternalErrorResponse();
			}

			return response;
		}
		public async Task<ReturnResponse> GetPackById(int id)
		{
			ReturnResponse response;
			_logger.LogInformation("Search pack in database{}", id);
			try
			{
				var pack = await _dbContext.Packs.FindAsync(id);
				if (pack != null)
				{
					response = new OkResponse<Pack>(pack);
				}
				else
				{
					response = new NotFoundResponse();
				}
			}
			catch (Exception ex)
			{
				response = new InternalErrorResponse();
			}

			return response;
		}
		public async Task<ReturnResponse> GetPacksByInvestId(int investId)
		{
			ReturnResponse response;
			try
			{
				var packs = _dbContext.Packs.Where(p => p.InvestmentId == investId).ToList();
				if (packs != null)
				{
					response = new OkResponse<List<Pack>>(packs);
				}
				else
				{
					response = new NotFoundResponse();
				}
			}
			catch (Exception ex)
			{
				response = new InternalErrorResponse();
			}

			return response;
		}
		public async Task<ReturnResponse> GetPacksByInvestCode(string investCode)
		{
			ReturnResponse response;
			try
			{
				var packs = _dbContext.Packs
					.Include(p => p.PackType)
					.Include(p => p.Investment.Promocode)
					.Where(p => p.Investment.Promocode.Code == investCode)
					.ToList();
				if (packs != null)
				{
					response = new OkResponse<List<Pack>>(packs);
				}
				else
				{
					response = new NotFoundResponse();
				}
			}
			catch (Exception ex)
			{
				response = new InternalErrorResponse();
			}

			return response;
		}
		public async Task<List<PackType>> GetAllPackTypes()
		{
			try
			{
				return _dbContext.PackTypes.ToList();
			}
			catch { return null; }
		}
	}
}
