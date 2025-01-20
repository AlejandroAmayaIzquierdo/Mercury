using Microsoft.EntityFrameworkCore;

namespace Mercury.Models.Auth;

[Index(nameof(UserName), IsUnique = true)]
public class User
{
    public Guid Id { get; set; }
    public string UserName { get; set; } = string.Empty;
    public string PasswordHash { get; set; } = string.Empty;

    // TODO implement a role pyramid or be able to have multiple roles one user.
    public string Role { get; set; } = string.Empty;

    public string? RefreshToken { get; set; }
    public DateTime? RefreshTokenExpiryTime { get; set; }
}
