using Microsoft.EntityFrameworkCore;

namespace Mercury.Models.Auth;

[Index(nameof(UserName), IsUnique = true)]
public class User
{
    public Guid Id { get; set; }
    public string UserName { get; set; } = string.Empty;
    public string PasswordHash { get; set; } = string.Empty;

    public virtual ICollection<UserRole> UserRoles { get; set; } = [];

    public string? RefreshToken { get; set; }
    public DateTime? RefreshTokenExpiryTime { get; set; }
}
