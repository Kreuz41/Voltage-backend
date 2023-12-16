namespace FP.Core.Api.ApiDto
{
	public class PackDto
	{
		public decimal DealSum { get; set; }
		public int PackTypeId { get; set; }
		public DateTime EndDate { get; set; }
		public string InvestmentCode { get; set; } = null!;
	}
}
