namespace Knights.Infrastructure.Tests.Security;

using System.Security.Claims;
using Knights.Infrastructure.Security;
using Microsoft.AspNetCore.Http;

public sealed class HttpTenantContextTests
{
    [Fact]
    public void TenantId_ReturnsGuid_WhenValidTenantIdClaimPresent()
    {
        var tenantId = Guid.NewGuid();
        var accessor = CreateAccessorWithClaim("tenant_id", tenantId.ToString());
        var context = new HttpTenantContext(accessor);

        var result = context.TenantId;

        Assert.Equal(tenantId, result);
    }

    [Fact]
    public void TenantId_ReturnsNull_WhenTenantIdClaimAbsent()
    {
        var accessor = CreateAccessorWithClaims(new[] { new Claim("sub", Guid.NewGuid().ToString()) });
        var context = new HttpTenantContext(accessor);

        var result = context.TenantId;

        Assert.Null(result);
    }

    [Fact]
    public void TenantId_ReturnsNull_WhenTenantIdClaimIsNotValidGuid()
    {
        var accessor = CreateAccessorWithClaim("tenant_id", "not-a-guid");
        var context = new HttpTenantContext(accessor);

        var result = context.TenantId;

        Assert.Null(result);
    }

    [Fact]
    public void TenantId_ReturnsNull_WhenHttpContextIsNull()
    {
        var accessor = CreateAccessorWithNullContext();
        var context = new HttpTenantContext(accessor);

        var result = context.TenantId;

        Assert.Null(result);
    }

    private static IHttpContextAccessor CreateAccessorWithClaim(string claimType, string claimValue)
    {
        return CreateAccessorWithClaims(new[] { new Claim(claimType, claimValue) });
    }

    private static IHttpContextAccessor CreateAccessorWithClaims(IEnumerable<Claim> claims)
    {
        var identity = new ClaimsIdentity(claims);
        var principal = new ClaimsPrincipal(identity);
        var httpContext = new DefaultHttpContext { User = principal };

        return new HttpContextAccessor { HttpContext = httpContext };
    }

    private static IHttpContextAccessor CreateAccessorWithNullContext()
    {
        return new HttpContextAccessor { HttpContext = null };
    }
}
