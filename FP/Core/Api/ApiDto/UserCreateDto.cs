namespace FP.Core.Api.ApiDto;

public class UserCreateDto
{
	public string Password { get; set; } = "";
	public string Email { get; set; } = "";
	public string ReferrerCode { get; set; } = null!;

	public override string ToString()
	{
		return $"ObjectId - {Email} | Type - UserDto";
	}
}
