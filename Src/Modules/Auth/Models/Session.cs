using System.ComponentModel.DataAnnotations;
using System.Text.Json.Serialization;

namespace Mercury.Models.Auth;

public class Session
{
    [Key]
    public required Guid Id { get; set; }
    public required string AccessToken { get; init; }
    public required Guid UserId { get; init; }
    public string? RefreshToken { get; set; }
    public DateTime? RefreshTokenExpiryTime { get; set; }

    // public string DeviceId { get; init; } = string.Empty;

    // [JsonIgnore]
    // public virtual Device? Device { get; set; }
}
