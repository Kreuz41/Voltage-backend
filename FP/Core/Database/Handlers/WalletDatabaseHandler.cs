using FP.Core.Api.Providers.Interfaces;
using FP.Core.Database.Models;
using Microsoft.EntityFrameworkCore;
using TronNet;

namespace FP.Core.Database.Handlers;

public class WalletDatabaseHandler
{
    private readonly ILogger<WalletDatabaseHandler> _logger;
    private readonly FpDbContext _dbContext;
    private readonly IServiceProvider _serviceProvider;
    private readonly ICryptoApiTrc20Provider _cryptoApiProvider;
    private readonly TransactionDatabaseHandler _transactionDatabaseHandler;

    public WalletDatabaseHandler(FpDbContext dbContext, IServiceProvider service, ILogger<WalletDatabaseHandler> logger, ICryptoApiTrc20Provider cryptoApiProvider,
        TransactionDatabaseHandler transactionDatabaseHandler)
    {
        _dbContext = dbContext;
        _serviceProvider = service;
        _logger = logger;
        _cryptoApiProvider = cryptoApiProvider;
        _transactionDatabaseHandler = transactionDatabaseHandler;
    }

	public async Task<Wallet> CreateWallet()
	{
		_logger.LogInformation("Start to add wallet in database");

		Wallet wallet = new();
		var key = TronECKey.GenerateKey(TronNetwork.MainNet);
		var address = key.GetPublicAddress();

		if (address != null)
		{
			wallet.WalletAddress = address;
			wallet.WalletSecretKey = key.GetPrivateKey();
		}

        try
        {
            var result = await _dbContext.Wallets.AnyAsync(u => u.WalletAddress == wallet.WalletAddress);
            if (!result)
            {
                await _dbContext.Wallets.AddAsync(wallet);
                await _dbContext.SaveChangesAsync();
                _logger.LogInformation("Wallet created");
            }
            else
            {
                _logger.LogInformation($"Cannot create user with email {wallet.WalletAddress}");
            }
        }
        catch (Exception ex)
        {
            _logger.LogInformation(ex, "Cannot create wallet");
        }

        return wallet;
    }
    
    public async Task<Wallet?> GetWallet(string walletAddress)
    {
        _logger.LogInformation("Start to finding wallet in database");

        try
        {
            return await _dbContext.Wallets.FirstOrDefaultAsync(u => u.WalletAddress == walletAddress);
        }
        catch (Exception ex)
        {
            _logger.LogInformation(ex, "Cannot find wallet");        
            return null;
        }
    }
}