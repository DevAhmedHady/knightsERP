using Knights.Application.Common.Interfaces;
using Knights.Domain.Identity;

namespace Knights.Application.Tests.Fakes;

public sealed class FakeJwtTokenGenerator : IJwtTokenGenerator
{
    public JwtToken Generate(User user, Guid? tenantId, string? tenantCodeName)
        => new($"token-for-{user.Id}", DateTime.UtcNow.AddMinutes(60));
}
