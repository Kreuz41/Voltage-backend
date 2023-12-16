using FP.Core.Api.ApiDto;
using FP.Core.Api.Providers.Interfaces;
using FP.Core.Api.Responses;
using Microsoft.AspNetCore.Identity;
using System.Net;
using System.Text.Json;
using FP.Core.Database.Handlers;
using FP.Core.Database.Models;

namespace FP.Core.Api.Providers.Providers.Networks.TRC20
{
	public class CryptoApiTrc20Provider : ICryptoApiTrc20Provider
	{
		private readonly IHttpClientFactory _httpClientFactory;
		private static readonly JsonSerializerOptions Options = new(JsonSerializerDefaults.Web);
		private readonly IServiceProvider _serviceProvider;
		private readonly TransactionDatabaseHandler _transactionDatabaseHandler;

		public CryptoApiTrc20Provider(IHttpClientFactory httpClientFactory, IServiceProvider serviceProvider, TransactionDatabaseHandler dbHandler)
		{
			_httpClientFactory = httpClientFactory;
			_serviceProvider = serviceProvider;
			_transactionDatabaseHandler = dbHandler;
		}
		
		public async Task<CryptoCreatedWallet?> CreateTrc20Wallet()
		{
			var httpClient = _httpClientFactory.CreateClient("Crypto");
			using var response = await httpClient.PostAsync("create_trc20_wallet", null);
			if (response.StatusCode != HttpStatusCode.OK) return null;
			return await response.Content.ReadFromJsonAsync<CryptoCreatedWallet>();
		}

		public async Task<decimal?> GetTrc20WalletBalance(string walletAddress)
		{
			var httpClient = _httpClientFactory.CreateClient("Crypto");
			using var response = await httpClient.GetAsync($"trc20_balance/{walletAddress}");
			if (response.StatusCode != HttpStatusCode.OK) return null;
			var updated = await response.Content.ReadFromJsonAsync<CryptoWalletBalance>();
			return updated?.Balance;
		}
		
		public async Task<bool> TransferTrc20(Wallet fromWallet, Wallet toWallet, decimal amount)
		{
			try
			{
				var httpClient = _httpClientFactory.CreateClient("Crypto");

				var transferRequest = new
				{
					from = new { address = fromWallet.WalletAddress, privateKey = fromWallet.WalletSecretKey },
					to = new { address = toWallet.WalletAddress, privateKey = toWallet.WalletSecretKey },
					amount
				};

				using var content = new StringContent(JsonSerializer.Serialize(transferRequest, Options));
				using var response = await httpClient.PostAsync("transfer_trc20", content);
				return await response.Content.ReadFromJsonAsync<bool>();
			}
			catch (Exception ex)
			{
				return false;
			}
		}
		
		public async Task<bool> TransferNoFeeTrc20(Wallet fromWallet, string toWalletaddress, decimal amount)
		{
			try
			{
				var httpClient = _httpClientFactory.CreateClient("Crypto");

				var transferRequest = new
				{
					from = new { address = fromWallet.WalletAddress, privateKey = fromWallet.WalletSecretKey },
					to = new {address = toWalletaddress},
					amount
				};
				using var content = new StringContent(JsonSerializer.Serialize(transferRequest, Options));
				using var response = await httpClient.PostAsync("transfer_without_fee_trc20", content);
				return response.IsSuccessStatusCode;
			}
			catch (Exception ex)
			{
				return false;
			}
		}

		public async Task<bool> TransferCommissionTrc20(Wallet fromWallet, string toWalletAddress, int amount)
		{
			try
			{
				var httpClient = _httpClientFactory.CreateClient("Crypto");

				var transferRequest = new
				{
					from = new { address = fromWallet.WalletAddress, privateKey = fromWallet.WalletSecretKey },
					to = new {address = toWalletAddress},
					amount
				};
				using var content = new StringContent(JsonSerializer.Serialize(transferRequest, Options));
				using var response = await httpClient.PostAsync("transfer_commission_trc20", content);
				return response.IsSuccessStatusCode;
			}
			catch (Exception ex)
			{
				return false;
			}
		}
	}
}
