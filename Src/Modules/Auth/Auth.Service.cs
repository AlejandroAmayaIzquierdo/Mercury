using System.Text;
using Isopoh.Cryptography.Argon2;
using Mercury.Db;
using Mercury.Models.Auth;
using Mercury.Util;
using Microsoft.EntityFrameworkCore;

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

            User user =
                new()
                {
                    PasswordHash = hashedPassword,
                    Id = Guid.NewGuid(),
                    UserName = userDto.UserName,
                    Role = "User",
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
    public async Task<(string?, TokenResponseDto?)> LoginUserAsync(UserDto userDto)
    {
        User? user = await _dbContext.Users.FirstOrDefaultAsync(u =>
            u.UserName.ToLower() == userDto.UserName.ToLower()
        );

        bool isCredentialsWrong = false;

        isCredentialsWrong =
            user is null
            || !Argon2.Verify(user.PasswordHash, Encoding.UTF8.GetBytes(userDto.Password));

        if (isCredentialsWrong)
            return ("The user or the password is wrong", null);

        string refreshToken = await GenerateAndSaveRefreshTokenAsync(user!);

        return (null, await CreateTokenResponse(user!));
    }

    public async Task<User?> ValidateRefreshTokenAsync(RefreshTokenRequestDto dto)
    {
        var user = await _dbContext.Users.FindAsync(dto.UserId);
        if (
            user is null
            || user.RefreshToken != dto.RefreshToken
            || user.RefreshTokenExpiryTime <= DateTime.UtcNow
        )
            return null;
        return user;
    }

    public async Task<TokenResponseDto> CreateTokenResponse(User user)
    {
        string refreshToken = await GenerateAndSaveRefreshTokenAsync(user!);

        return new() { AccessToken = _jwtHandler.CreateToken(user!), RefreshToken = refreshToken };
    }

    private async Task<string> GenerateAndSaveRefreshTokenAsync(User user)
    {
        var refreshToken = _jwtHandler.GenerateRefreshToken();
        user.RefreshToken = refreshToken;
        user.RefreshTokenExpiryTime = DateTime.UtcNow.AddDays(1);

        _dbContext.Users.Update(user);

        await _dbContext.SaveChangesAsync();
        return refreshToken;
    }
}
