using System.Text.Json.Serialization;

namespace Mercury.Models.Auth;

public class UserRole
{
    public int Id { get; set; }
    public Guid UserId { get; set; }
    public int RoleId { get; set; }

    [JsonIgnore]
    public virtual Role? Role { get; set; }

    [JsonIgnore]
    public virtual User? User { get; set; }
}
