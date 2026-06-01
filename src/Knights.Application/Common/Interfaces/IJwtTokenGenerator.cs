using Knights.Domain.Identity;

namespace Knights.Application.Common.Interfaces;

public sealed record JwtToken(string Token, DateTime ExpiresAtUtc);

public interface IJwtTokenGenerator
{
    JwtToken Generate(User user, Guid? tenantId, string? tenantCodeName);
}
