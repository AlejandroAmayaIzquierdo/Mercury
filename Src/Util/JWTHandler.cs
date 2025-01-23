using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Security.Cryptography;
using System.Text;
using Mercury.Models.Auth;
using Microsoft.IdentityModel.Tokens;

namespace Mercury.Util;

public class JWTHandler(IConfiguration configuration)
{
    private readonly IConfiguration _configuration = configuration;

    public string CreateToken(User user)
    {
        var permissions = user
            .UserRoles.SelectMany(ur => ur.Role?.RolePermissions ?? [])
            .Select(rp => rp.Permission.Id)
            .Distinct()
            .ToList();

        LogService.Get()?.Info($"Permisions {string.Join(",", permissions)}");

        var claims = new List<Claim>
        {
            new(ClaimTypes.NameIdentifier, user.Id.ToString()),
            new(ClaimTypes.Name, user.UserName),
            new(ClaimTypes.Role, string.Join(",", user.UserRoles.Select(ur => ur.Role?.Name))),
            new("permissions", string.Join(",", permissions))
        };

        var key = new SymmetricSecurityKey(
            Encoding.UTF8.GetBytes(_configuration.GetValue<string>("JWTSecurity:Token")!)
        );

        var creds = new SigningCredentials(key, SecurityAlgorithms.HmacSha512);

        var tokenDescriptor = new JwtSecurityToken(
            issuer: _configuration.GetValue<string>("JWTSecurity:Issuer"),
            audience: _configuration.GetValue<string>("JWTSecurity:Audience"),
            claims: claims,
            expires: DateTime.UtcNow.AddMinutes(10),
            signingCredentials: creds
        );

        return new JwtSecurityTokenHandler().WriteToken(tokenDescriptor);
    }

    public string GenerateRefreshToken()
    {
        var randomNumber = new byte[32];
        using var rng = RandomNumberGenerator.Create();
        rng.GetBytes(randomNumber);
        return Convert.ToBase64String(randomNumber);
    }
}
