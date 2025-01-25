using Mercury.Models.Auth;
using Mercury.Util;
using Microsoft.AspNetCore.Mvc;

namespace Mercury.Modules.Auth;

public class AuthModule : BaseModuleHandler
{
    protected override string MODULE => "/Auth";

    public override void Register(ref RouteGroupBuilder module)
    {
        module.MapPost(
            "/register",
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
            async ([FromBody] UserDto request, AuthService service, HttpContext context) =>
            {
                var resp = await service.LoginUserAsync(request);

                string? err = resp.Item1;
                TokenResponseDto? tokenResponse = resp.Item2;

                if (err != null)
                    return Results.BadRequest(err);

                return Results.Ok(tokenResponse);
            }
        );

        module.MapPost(
            "/refresh-token",
            async ([FromBody] RefreshTokenRequestDto request, AuthService service) =>
            {
                var user = await service.GetUserByIdAsync(request.UserId);
                var isTokenValid = await service.ValidateRefreshTokenAsync(request);

                if (user is null)
                    return Results.Unauthorized();

                if (!isTokenValid)
                    return Results.Unauthorized();

                return Results.Ok(await service.GenerateSessionAndSaveRefreshTokenAsync(user));
            }
        );
    }
}
