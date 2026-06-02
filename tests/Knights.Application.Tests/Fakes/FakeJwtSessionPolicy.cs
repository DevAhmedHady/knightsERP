using Knights.Application.Common.Interfaces;
using Knights.Domain.Identity;

namespace Knights.Application.Tests.Fakes;

public sealed class FakeJwtSessionPolicy : IJwtSessionPolicy
{
    public Task<int> ResolveEffectiveSessionTimeoutMinutesAsync(User user, Guid? tenantId, CancellationToken cancellationToken = default)
    {
        return Task.FromResult(user.SessionTimeoutMinutes ?? 60);
    }
}
