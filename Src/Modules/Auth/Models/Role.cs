using System.Text.Json.Serialization;

namespace Mercury.Models.Auth;

public class Role
{
    public int Id { get; set; } = 0;
    public string Name { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public ICollection<RolePermission> RolePermissions { get; set; } = [];
    public bool Active { get; set; } = true;

    // Navigation properties
    [JsonIgnore]
    public virtual ICollection<UserRole> UserRoles { get; set; } = [];

    [JsonIgnore]
    public virtual ICollection<Permission> Permissions { get; set; } = [];
}
