using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using Knights.Application.Common.Interfaces;
using Knights.Domain.Identity;
using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.Tokens;

namespace Knights.Infrastructure.Security;

public sealed class JwtTokenGenerator(IOptions<JwtOptions> options) : IJwtTokenGenerator
{
    private readonly JwtOptions _options = options.Value;

    public JwtToken Generate(User user, Guid? tenantId, string? tenantCodeName, int expiryMinutes)
    {
        var expiresAtUtc = DateTime.UtcNow.AddMinutes(expiryMinutes);

        var claims = new List<Claim>
        {
            new Claim(JwtRegisteredClaimNames.Sub, user.Id.ToString()),
            new Claim(JwtRegisteredClaimNames.Email, user.Email),
            new Claim(JwtRegisteredClaimNames.Name, user.UserName),
            new Claim(JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToString())
        };

        if (tenantId is not null)
        {
            claims.Add(new Claim("tenant_id", tenantId.Value.ToString()));
        }

        if (!string.IsNullOrWhiteSpace(tenantCodeName))
        {
            claims.Add(new Claim("tenant_code", tenantCodeName));
        }

        var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_options.SecretKey));
        var credentials = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);

        var token = new JwtSecurityToken(
            issuer: _options.Issuer,
            audience: _options.Audience,
            claims: claims,
            notBefore: DateTime.UtcNow,
            expires: expiresAtUtc,
            signingCredentials: credentials);

        return new JwtToken(new JwtSecurityTokenHandler().WriteToken(token), expiresAtUtc);
    }
}
