using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;

using Microsoft.IdentityModel.Tokens;

namespace Medium.Api.Infrastructure.Auth;

public class JwtTokenGenerator(IConfiguration configuration) : IJwtTokenGenerator
{
  private readonly string _secretKey = configuration["Jwt:Key"] ?? throw new ArgumentNullException("Jwt:Key");
  private readonly string _issuer = configuration["Jwt:Issuer"] ?? "Medium.Api";
  private readonly string _audience = configuration["Jwt:Audience"] ?? "Medium.Api";
  private readonly int _expiryMinutes = int.Parse(configuration["Jwt:AccessTokenExpirationMinutes"] ?? "60");

  public string GenerateToken(Guid userId, string email, IEnumerable<string> roles, IEnumerable<string> permissions)
  {
    var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_secretKey));
    var credentials = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);

    var claims = new List<Claim>
      {
        new(JwtRegisteredClaimNames.Sub, userId.ToString()),
        new(JwtRegisteredClaimNames.Email, email),
        new(JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToString())
      };

    foreach (var role in roles)
    {
      claims.Add(new Claim(ClaimTypes.Role, role));
    }

    foreach (var permission in permissions.Distinct(StringComparer.OrdinalIgnoreCase))
    {
      claims.Add(new Claim(PermissionClaimTypes.Permission, permission));
    }

    var token = new JwtSecurityToken(
        issuer: _issuer,
        audience: _audience,
        claims: claims,
        expires: DateTime.UtcNow.AddMinutes(_expiryMinutes),
        signingCredentials: credentials
    );

    return new JwtSecurityTokenHandler().WriteToken(token);
  }
}