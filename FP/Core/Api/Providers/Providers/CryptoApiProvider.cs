using FP.Core.Api.Providers.Interfaces;
using FP.Core.Api.Responses;

namespace FP.Core.Api.Providers.Providers;

public class CryptoApiProvider : ICryptoApiProvider
{
	public Task<CryptoCreatedWallet?> CreateWallet()
	{
		throw new NotImplementedException();
	}

	public Task<decimal?> GetWalletBalance(string walletAddress)
	{
		throw new NotImplementedException();
	}
}
