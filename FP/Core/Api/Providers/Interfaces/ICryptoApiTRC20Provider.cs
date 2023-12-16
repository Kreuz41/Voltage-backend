using FP.Core.Api.ApiDto;
using FP.Core.Api.Responses;
using FP.Core.Database.Models;

namespace FP.Core.Api.Providers.Interfaces;

public interface ICryptoApiTrc20Provider
{
	public Task<CryptoCreatedWallet?> CreateTrc20Wallet();

    public Task<decimal?> GetTrc20WalletBalance(string walletAddress);
    public Task<bool> TransferTrc20(Wallet fromWallet, Wallet toWallet, decimal amount);
	public Task<bool> TransferNoFeeTrc20(Wallet fromWallet, string toWalletaddress, decimal amount);
	public Task<bool> TransferCommissionTrc20(Wallet fromWallet, string toWalletAddress, int amount);
}