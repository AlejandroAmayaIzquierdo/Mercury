using Mercury.Models.Auth;
using Mercury.Util;
using Microsoft.AspNetCore.Mvc;
using UAParser;

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
                var userAgent = context.Request.Headers.UserAgent;
                var ipAddress = context.Connection.RemoteIpAddress?.ToString();

                if (string.IsNullOrEmpty(userAgent) || string.IsNullOrEmpty(ipAddress))
                    return Results.BadRequest();

                var device = await service.RegisterDevice(
                    new() { IpAddress = ipAddress, UserAgent = userAgent! }
                );

                if (device == null)
                    return Results.BadRequest();

                var resp = await service.LoginUserAsync(request, device);

                string? err = resp.Item1;
                TokenResponseDto? tokenResponse = resp.Item2;

                if (err != null)
                    return Results.BadRequest(err);

                return Results.Ok(tokenResponse);
            }
        );

        module.MapPost(
            "/refresh-token",
            async (
                [FromBody] RefreshTokenRequestDto request,
                AuthService service,
                HttpContext context
            ) =>
            {
                var userAgent = context.Request.Headers.UserAgent;
                var ipAddress = context.Connection.RemoteIpAddress?.ToString();

                if (string.IsNullOrEmpty(userAgent) || string.IsNullOrEmpty(ipAddress))
                    return Results.BadRequest();

                var deviceRequest = service.BuildDevice(
                    new() { IpAddress = ipAddress, UserAgent = userAgent! }
                );

                bool isValidDevice = await service.ValidateDevice(
                    deviceRequest,
                    request.ExpiredAccessToken
                );

                // TODO When mailer its added. Send a email to the user that another device try to access his account.
                if (!isValidDevice)
                    return Results.BadRequest("Unauthorized device");

                var user = await service.GetUserByIdAsync(request.UserId);
                var isTokenValid = await service.ValidateRefreshTokenAsync(request);

                if (user is null || !isTokenValid)
                    return Results.Unauthorized();

                return Results.Ok(
                    await service.GenerateSessionAndSaveRefreshTokenAsync(
                        user,
                        deviceRequest,
                        request.ExpiredAccessToken
                    )
                );
            }
        );
    }
}
