using FP.Core.Api.Responses;

namespace FP.Core.Api.Providers.Interfaces
{
	public interface ICryptoApiProvider
	{
		public Task<CryptoCreatedWallet?> CreateWallet();

		public Task<decimal?> GetWalletBalance(string walletAddress);
	}
}
