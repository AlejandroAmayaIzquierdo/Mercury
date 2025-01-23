using System.Text.Json.Serialization;

namespace Mercury.Models.Auth;

public class RolePermission
{
    public int Id { get; set; }
    public int RoleId { get; set; }
    public int PermissionId { get; set; }

    // Navigation properties
    [JsonIgnore]
    public virtual Role? Role { get; set; }

    [JsonIgnore]
    public virtual Permission? Permission { get; set; }
}
