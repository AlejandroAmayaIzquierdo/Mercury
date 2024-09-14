
using Mercury.DB;

namespace Mercury.Routes.Dev;

public class DevRoute : IRoute
{
    public string MODULE => "/Dev";

    public void Register(ref WebApplication app)
    {
        app.MapGet($"{MODULE}/connection", (MySqliteContext dbContext) =>
        {
            var a = dbContext.Users.Where(e => e.UserID == 1).Take(1);
            foreach (var item in a)
                Console.WriteLine(item.Rol);
            return Results.Ok("😎👍");
        });
    }
}
