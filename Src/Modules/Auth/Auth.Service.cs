using System.Text;
using Isopoh.Cryptography.Argon2;
using Mercury.Db;
using Mercury.Models.Auth;
using Mercury.Util;
using Microsoft.EntityFrameworkCore;
using UAParser;

namespace Mercury.Modules.Auth;

public class AuthService(MysqlContext dbContext, JWTHandler jwtHandler)
{
    private readonly MysqlContext _dbContext = dbContext;
    private readonly JWTHandler _jwtHandler = jwtHandler;

    public async Task<(string?, User?)> RegisterUserAsync(UserDto userDto)
    {
        try
        {
            if (await _dbContext.Users.AnyAsync(u => u.UserName == userDto.UserName))
                return ("The username is already taken.", null);

            string pass = userDto.Password;

            // TODO add validations of password complexity
            if (string.IsNullOrEmpty(pass) || pass.Length < 8)
                return ("The password length should be at least 8 characters", null);

            var hashedPassword = Argon2.Hash(userDto.Password);

            var userId = Guid.NewGuid();

            User user =
                new()
                {
                    PasswordHash = hashedPassword,
                    Id = userId,
                    UserName = userDto.UserName,
                    UserRoles = [new UserRole() { UserId = userId, RoleId = 2 }] // XXX Hardcoded 'User' Role
                };

            _dbContext.Users.Add(user);

            await _dbContext.SaveChangesAsync();
            return (null, user);
        }
        catch (Exception ex)
        {
            return (ex.Message, null);
        }
    }

    /// <summary>
    /// Validates if a user exists and if the provided password is valid.
    /// </summary>
    /// <param name="userDto">An object containing the user's username and password.</param>
    /// <returns>
    /// A tuple where the first element is an error message (if any) and the second element is a success message or token.
    /// - If validation fails: returns a tuple with an error message as the first element and null as the second element.
    /// - If validation succeeds: returns null as the first element and a success token as the second element.
    /// </returns>
    public async Task<(string?, TokenResponseDto?)> LoginUserAsync(
        UserDto userDto,
        Models.Auth.Device device
    )
    {
        var user = await _dbContext
            .Users.Include(u => u.UserRoles)
            .ThenInclude(ur => ur.Role)
            .ThenInclude(rp => rp.RolePermissions)
            .ThenInclude(p => p.Permission)
            .FirstOrDefaultAsync(u =>
                u.UserName.Equals(userDto.UserName, StringComparison.CurrentCultureIgnoreCase)
            );

        bool isCredentialsWrong = false;

        isCredentialsWrong =
            user is null
            || !Argon2.Verify(user.PasswordHash, Encoding.UTF8.GetBytes(userDto.Password));

        if (isCredentialsWrong)
            return ("The user or the password is wrong", null);

        TokenResponseDto response = await GenerateSessionAndSaveRefreshTokenAsync(user!, device);

        return (null, response);
    }

    public async Task<bool> ValidateRefreshTokenAsync(RefreshTokenRequestDto dto)
    {
        var session = await _dbContext.Sessions.FirstOrDefaultAsync(s =>
            s.UserId == dto.UserId && s.AccessToken == dto.ExpiredAccessToken
        );

        if (
            session is null
            || session.RefreshToken != dto.RefreshToken
            || session.RefreshTokenExpiryTime <= DateTime.UtcNow
        )
            return false;

        return true;
    }

    public async Task<TokenResponseDto> GenerateSessionAndSaveRefreshTokenAsync(
        User user,
        Models.Auth.Device? device = null,
        string? ExpiredAccessToken = null
    )
    {
        string newAccessToken = _jwtHandler.CreateToken(user!);

        var refreshToken = _jwtHandler.GenerateRefreshToken();

        Session? session = await _dbContext.Sessions.FirstOrDefaultAsync(s =>
            s.UserId == user.Id && s.AccessToken == ExpiredAccessToken
        );
        if (session == null)
        {
            session = new Session()
            {
                Id = Guid.NewGuid(),
                AccessToken = newAccessToken,
                UserId = user.Id,
                RefreshToken = refreshToken,
                RefreshTokenExpiryTime = DateTime.UtcNow.AddDays(1),
                DeviceId = device?.DeviceId
            };
            _dbContext.Sessions.Add(session);
        }
        else
        {
            session.RefreshToken = refreshToken;
            session.RefreshTokenExpiryTime = DateTime.UtcNow.AddDays(1);

            _dbContext.Sessions.Update(session);
        }

        await _dbContext.SaveChangesAsync();
        return new() { AccessToken = newAccessToken, RefreshToken = refreshToken };
    }

    public async Task<Models.Auth.Device?> RegisterDevice(RegisterDeviceDto dto)
    {
        try
        {
            var parser = Parser.GetDefault();
            var clientInfo = parser.Parse(dto.UserAgent);

            var deviceName = $"{clientInfo.OS} - {clientInfo.Device.Family}";

            var device = new Models.Auth.Device()
            {
                DeviceId = Guid.NewGuid(),
                UserAgent = dto.UserAgent,
                DeviceName = deviceName,
                IPAddress = dto.IpAddress,
            };

            _dbContext.Devices.Add(device);

            await _dbContext.SaveChangesAsync();

            return device;
        }
        catch (Exception ex)
        {
            LogService.Get()?.Error(ex.Message);
            return null;
        }
    }

    public Models.Auth.Device BuildDevice(RegisterDeviceDto dto)
    {
        var parser = Parser.GetDefault();
        var clientInfo = parser.Parse(dto.UserAgent);

        var deviceName = $"{clientInfo.OS} - {clientInfo.Device.Family}";

        var device = new Models.Auth.Device()
        {
            UserAgent = dto.UserAgent,
            DeviceName = deviceName,
            IPAddress = dto.IpAddress,
        };
        return device;
    }

    public async Task<bool> ValidateDevice(Models.Auth.Device deviceRequest, string accessToken)
    {
        var session = await _dbContext
            .Sessions.Include(s => s.Device)
            .FirstOrDefaultAsync(s => s.AccessToken == accessToken);
        if (session is null)
            return false;

        var device = session.Device;

        if (device is null)
            return false;

        if (
            device.UserAgent != deviceRequest.UserAgent
            || device.IPAddress != deviceRequest.IPAddress
        )
            return false;
        return true;
    }

    public async Task<User?> GetUserByIdAsync(Guid id)
    {
        return await _dbContext
            .Users.Include(u => u.UserRoles)
            .ThenInclude(ur => ur.Role)
            .ThenInclude(rp => rp.RolePermissions)
            .ThenInclude(p => p.Permission)
            .FirstOrDefaultAsync(u => u.Id == id);
    }

    public async Task<User?> AssignRoleToUserAsync(Guid userID, params int[] rolesId)
    {
        User? user = await _dbContext
            .Users.Include(u => u.UserRoles)
            .FirstOrDefaultAsync(u => u.Id == userID);

        if (user is null)
            return null;

        foreach (var roleId in rolesId)
        {
            if (user.UserRoles.Any(ur => ur.RoleId == roleId))
                continue;

            user.UserRoles.Add(new UserRole() { UserId = userID, RoleId = roleId });
        }

        await _dbContext.SaveChangesAsync();

        return user;
    }
}
