using System.ComponentModel.DataAnnotations.Schema;
using System.Text.Json.Serialization;
using Org.BouncyCastle.Pkix;

namespace FP.Core.Database.Models;
public class Investment
{
	public int Id { get; set; }
	public decimal TotalSum { get; set; }
	public decimal MaxSum { get; set; }
	public decimal TotalYield { get; set; }
	public bool IsEnded { get; set; } = false;
	public DateTime StartDate { get; set; } = DateTime.UtcNow;
	public DateTime EndDate { get; set; }
	public int PacksCount { get; set; } = 0;
	public decimal TotalAccrual { get; set; }

	public int UserId { get; set; }
	[ForeignKey("UserId")]
	public User User { get; set; } = null!;
	
	public int CodeId { get; set; }
	[ForeignKey("CodeId")]
	public Promocode Promocode { get; set; } = null!;
	
	[JsonIgnore] public ICollection<Pack> Packs { get; set; } = new List<Pack>();
}


