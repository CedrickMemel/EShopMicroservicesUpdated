using Microsoft.IdentityModel.Tokens;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;

namespace IdentityService.Services;

public class JwtTokenService(IConfiguration config) : IJwtTokenService
{
    private readonly byte[] _key = Encoding.UTF8.GetBytes(
       config["Jwt:Key"] ?? throw new InvalidOperationException("Jwt:Key manquant"));

    private readonly string _issuer = config["Jwt:Issuer"]
        ?? throw new InvalidOperationException("Jwt:Issuer manquant");

    private readonly string _audience = config["Jwt:Audience"]
        ?? throw new InvalidOperationException("Jwt:Audience manquant");

    public Task<string> GenerateTokenAsync(IdentityUser user, string[] roles)
    {
        var claims = new List<Claim>
        {
            new(JwtRegisteredClaimNames.Sub, user.Id),
            new(ClaimTypes.NameIdentifier, user.Id),
            new(ClaimTypes.Name, user.UserName ?? string.Empty),
            new(JwtRegisteredClaimNames.Email, user.Email ?? string.Empty)
        };

        foreach (var r in roles)
            claims.Add(new Claim(ClaimTypes.Role, r));

        var token = CreateToken(claims, TimeSpan.FromHours(3));
        return Task.FromResult(token);
    }

    public string GenerateClientToken(string clientId, string[] scopes)
    {
        var claims = new List<Claim>
        {
            new("client_id", clientId)
        };
        foreach (var s in scopes) claims.Add(new Claim("scope", s));
        return CreateToken(claims, TimeSpan.FromHours(1));
    }

    private string CreateToken(IEnumerable<Claim> claims, TimeSpan expiry)
    {
        var signingKey = new SymmetricSecurityKey(_key);
        var creds = new SigningCredentials(signingKey, SecurityAlgorithms.HmacSha256);

        var token = new JwtSecurityToken(
            issuer: _issuer,
            audience: _audience,
            claims: claims,
            notBefore: DateTime.UtcNow,
            expires: DateTime.UtcNow.Add(expiry),
            signingCredentials: creds
        );

        return new JwtSecurityTokenHandler().WriteToken(token);
    }
}