
using Mercury.DB;
using Microsoft.EntityFrameworkCore;

namespace Mercury.Routes.Dev;

public class DevRoute : IRoute
{
    public string MODULE => "/Dev";

    public void Register(ref WebApplication app)
    {
        app.MapGet($"{MODULE}/connection", (MySqliteContext dbContext) =>
        {
            var usersAdmins = dbContext.Users
                .Include(m => m.Rol)
                .Select(user => new { user.Rol.RolID, user.UserName })
                .Where(user => user.RolID == 2)
                .ToList();
            return Results.Ok(usersAdmins);
        });
    }
}
