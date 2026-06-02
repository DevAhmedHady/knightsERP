using Knights.Application.Common.Interfaces;
using Knights.Domain.Identity;
using Microsoft.Extensions.Options;

namespace Knights.Infrastructure.Security;

public sealed class JwtSessionPolicy(
    ITenantRepository tenantRepository,
    IOptions<JwtOptions> options) : IJwtSessionPolicy
{
    private readonly JwtOptions _options = options.Value;

    public async Task<int> ResolveEffectiveSessionTimeoutMinutesAsync(User user, Guid? tenantId, CancellationToken cancellationToken = default)
    {
        if (user.SessionTimeoutMinutes.HasValue)
            return user.SessionTimeoutMinutes.Value;

        if (tenantId.HasValue)
        {
            var tenant = await tenantRepository.GetByIdAsync(tenantId.Value, cancellationToken);
            if (tenant is not null)
                return tenant.SessionTimeoutMinutes;
        }

        return _options.ExpiryMinutes;
    }
}
