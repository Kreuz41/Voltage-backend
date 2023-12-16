using Newtonsoft.Json;

namespace FP.Core.Database.Models;

public class PackType
{
	public int Id { get; set; }
	public string Name { get; set; } = "";
	public decimal Yeild { get; set; }
	public int MaxDuration { get; set; }
	public int MinDuration { get; set; }

	[JsonIgnore] public ICollection<Pack> Packs = new List<Pack>();
}
