using Knights.Application.Common.Interfaces;
using Microsoft.AspNetCore.Http;

namespace Knights.Infrastructure.Security;

public sealed class HttpTenantContext : ITenantContext
{
    private const string TenantIdClaimType = "tenant_id";

    private readonly IHttpContextAccessor _httpContextAccessor;

    public HttpTenantContext(IHttpContextAccessor httpContextAccessor)
    {
        _httpContextAccessor = httpContextAccessor;
    }

    public Guid? TenantId
    {
        get
        {
            var tenantIdClaim = _httpContextAccessor.HttpContext?.User.FindFirst(TenantIdClaimType)?.Value;

            return Guid.TryParse(tenantIdClaim, out var tenantId)
                ? tenantId
                : null;
        }
    }
}
