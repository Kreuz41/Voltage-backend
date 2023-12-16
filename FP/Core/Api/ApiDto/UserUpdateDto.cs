namespace FP.Core.Api.ApiDto;

public class UserUpdateDto
{
    public string? Name { get; set; }
    public string? Surname { get; set; }
    public string? Nickname { get; set; }
    public string? Country { get; set; }
    public string? City { get; set; }
    public string? Phone { get; set; }
    public string? Telegram { get; set; }
    public string? Avatar { get; set; }
    
    public bool ShowPhone { get; set; }
    public bool ShowTg { get; set; }
    public bool ShowEmail { get; set; }
}