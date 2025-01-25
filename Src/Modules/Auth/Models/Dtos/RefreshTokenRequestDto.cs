namespace Mercury.Models.Auth;

public class RefreshTokenRequestDto
{
    public required Guid UserId { get; set; }
    public required string ExpiredAccessToken { get; set; }
    public required string RefreshToken { get; set; }
}
