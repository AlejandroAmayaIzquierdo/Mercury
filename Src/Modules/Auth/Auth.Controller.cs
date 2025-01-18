using Mercury.Models.Auth;
using Microsoft.AspNetCore.Mvc;

namespace Mercury.Modules.Auth;

public class AuthModule : BaseModuleHandler
{
    protected override string MODULE => "/Auth";

    public override void Register(ref RouteGroupBuilder module)
    {
        module.MapPost(
            "/Register",
            async ([FromBody] UserDto request, AuthService service) =>
            {
                var resp = await service.RegisterUserAsync(request);

                string? err = resp.Item1;
                User? user = resp.Item2;

                if (err != null)
                    return Results.BadRequest(err);

                return Results.Ok(user);
            }
        );

        module.MapPost(
            "/login",
            async ([FromBody] UserDto request, AuthService service) =>
            {
                var resp = await service.LoginUserAsync(request);

                string? err = resp.Item1;
                string? token = resp.Item2;

                if (err != null)
                    return Results.BadRequest(err);

                return Results.Ok(token);
            }
        );

        module
            .MapGet(
                "/authEndpoint",
                () =>
                {
                    return Results.Ok("😃");
                }
            )
            .RequireAuthorization();

        module
            .MapGet(
                "/authAdmin",
                () =>
                {
                    return Results.Ok("😎");
                }
            )
            .RequireAuthorization(options => options.RequireRole("Admin"));
    }
}
