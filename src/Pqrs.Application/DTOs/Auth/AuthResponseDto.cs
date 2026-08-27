namespace Pqrs.Application.DTOs.Auth;

public class AuthResponseDto
{
    public string AccessToken { get; set; } = string.Empty;
    public UserDto User { get; set; } = null!;
}
