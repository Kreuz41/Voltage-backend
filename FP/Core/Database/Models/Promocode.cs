using Newtonsoft.Json;

namespace FP.Core.Database.Models;

public class Promocode
{
    public int Id { get; set; }
    public string Code { get; set; } = "";
    public bool IsActivated { get; set; } = false;
}