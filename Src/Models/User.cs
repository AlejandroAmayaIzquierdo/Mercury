
namespace Mercury.Models;

public class User
{
    public int UserID { get; set; }
    public string? UserName { get; set; }

    // Foreign key property
    public int RolID { get; set; }

    // Navegation property
    public Rol Rol { get; set; } = null!;
}

public class Rol
{
    public int RolID { get; set; }
    public string RollName { get; set; } = string.Empty;
}