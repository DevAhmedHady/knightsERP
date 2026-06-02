using Knights.Domain.Identity;

namespace Knights.Application.Common.Interfaces;

public interface IJwtSessionPolicy
{
    Task<int> ResolveEffectiveSessionTimeoutMinutesAsync(User user, Guid? tenantId, CancellationToken cancellationToken = default);
}
