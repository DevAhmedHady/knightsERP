using Knights.Application.Common.Interfaces;
using Microsoft.AspNetCore.Http;

namespace Knights.Infrastructure.Security;

public sealed class HttpTenantContext(IHttpContextAccessor httpContextAccessor) : ITenantContext
{
    private const string TenantIdClaimType = "tenant_id";

    public Guid? TenantId
    {
        get
        {
            var tenantIdClaim = httpContextAccessor.HttpContext?.User.FindFirst(TenantIdClaimType)?.Value;

            return Guid.TryParse(tenantIdClaim, out var tenantId)
                ? tenantId
                : null;
        }
    }
}
