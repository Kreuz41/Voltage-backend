namespace FP.Core.Api.ApiDto
{
	public class WithdrawDto
	{
		public decimal Sum { get; set; }
		public string WalletAddress { get; set; }
		public bool FromAgentBalance { get; set; }
	}
}
