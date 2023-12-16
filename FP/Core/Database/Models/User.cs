﻿using System.ComponentModel.DataAnnotations.Schema;
using System.Text.Json.Serialization;

namespace FP.Core.Database.Models;

public class User
{
	[JsonIgnore]
	public int Id { get; set; }
	public int Rang { get; set; }
	[JsonIgnore] public string Passwordhash { get; set; } = "";
	public string Email { get; set; } = "";
	public decimal BalanceCrypto { get; set; } = 0m;
	public decimal BalanceAgent { get; set; } = 0m;
	public decimal BalanceIncome { get; set; } = 0m;
	public decimal TotalIncome { get; set; } = 0m;
	public decimal LinesIncome { get; set; } = 0m;
	public decimal CurrentIncome { get; set; }
	public bool IsVerified { get; set; } 

	public string ReferralCode { get; set; } = null!;
	public string ReferrerCode { get; set; } = null!;
	
	public string? Name { get; set; }
	public string? Surname { get; set; }
	public string? Nickname { get; set; }
	public string? Country { get; set; }
	public string? City { get; set; }
	public string? Phone { get; set; }
	public string? Telegram { get; set; }
	public string? Avatar { get; set; }
	public long? TelegramId { get; set; }
	
	[JsonIgnore] public bool IsAdmin { get; set; } = false;
	public bool IsRegistrationEnded { get; set; } = false;
	public DateTime RegistrationTime { get; set; } = DateTime.UtcNow;
	public DateTime LastActivityTime {  get; set; } = DateTime.UtcNow;
	
	public bool ShowPhone { get; set; } = true;
	public bool ShowTg { get; set; } = true;
	public bool ShowEmail { get; set; } = true;
	
	public int TopUpWalletId { get; set; }
	[ForeignKey("TopUpWalletId")]
	public Wallet TopUpWallet { get; set; } = null!;
	
	[JsonIgnore] public ICollection<Investment> Investments { get; set; } = new List<Investment>();
	[JsonIgnore] public ICollection<Pack> Packs { get; set; } = new List<Pack>();
	[JsonIgnore] public ICollection<Withdraw> Withdraws { get; set; } = new List<Withdraw>();
	[JsonIgnore] public ICollection<Referral> ReferrersCollection { get; set; } = new List<Referral>();
	[JsonIgnore] public ICollection<Referral> Referral { get; set; } = new List<Referral>();
}
