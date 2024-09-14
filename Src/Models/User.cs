
namespace Mercury.Models;

public class User
{
    public int UserID { get; set; }
    public string? UserName { get; set; }
    public int Rol { get; set; }
}

public class Rol
{
    public int RolID { get; set; }
    public string? RollName { get; set; }
}