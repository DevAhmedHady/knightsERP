using System.IdentityModel.Tokens.Jwt;
using Knights.Application.Common.Interfaces;
using Microsoft.AspNetCore.Http;
using System.Security.Claims;

namespace Knights.Infrastructure.Security;

public sealed class HttpUserContext(IHttpContextAccessor httpContextAccessor) : IUserContext
{
    public Guid UserId
    {
        get
        {
            var principal = httpContextAccessor.HttpContext?.User;
            var subject = principal?.FindFirst(JwtRegisteredClaimNames.Sub)?.Value
                ?? principal?.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            return Guid.TryParse(subject, out var userId)
                ? userId
                : throw new UnauthorizedAccessException("The authenticated user identifier is missing.");
        }
    }

    public Guid? TenantId
    {
        get
        {
            var value = httpContextAccessor.HttpContext?.User.FindFirst("tenant_id")?.Value;
            return Guid.TryParse(value, out var tenantId) ? tenantId : null;
        }
    }
}
