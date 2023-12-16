using System.Text.Json.Serialization;

namespace FP.Core.Database.Models;

public class Wallet
{
	public int Id { get; set; }
	public string WalletAddress { get; set; } = "";
	[JsonIgnore] 
	public string WalletSecretKey { get; set; } = "";
}
