using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using GadgetFix.Users.BLL.DTOs;
using Microsoft.IdentityModel.Tokens;

namespace GadgetFix.Users.Api;

public class JwtOptions
{
    public string Key { get; set; } = string.Empty;
    public string Issuer { get; set; } = "GadgetFix";
    public string Audience { get; set; } = "GadgetFix";
    public int ExpiresHours { get; set; } = 12;
}

public interface ITokenService
{
    string CreateToken(UserDto user);
}

public class TokenService(JwtOptions options) : ITokenService
{
    public string CreateToken(UserDto user)
    {
        var claims = new[]
        {
            new Claim(JwtRegisteredClaimNames.Sub, user.Id.ToString()),
            new Claim(ClaimTypes.Name, user.FullName),
            new Claim("phone", user.Phone),
            new Claim(ClaimTypes.Role, user.Role.ToString()),
        };

        var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(options.Key));
        var creds = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);

        var token = new JwtSecurityToken(
            issuer: options.Issuer,
            audience: options.Audience,
            claims: claims,
            expires: DateTime.UtcNow.AddHours(options.ExpiresHours),
            signingCredentials: creds);

        return new JwtSecurityTokenHandler().WriteToken(token);
    }
}
