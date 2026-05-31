using Knights.Domain.Exceptions;
using Knights.Domain.Tenants;

namespace Knights.Domain.Tests;

public class TenantTests
{
    [Fact]
    public void Create_WithNullExpiryDate_CreatesActiveTenant()
    {
        var tenant = Tenant.Create("main tenant", "Main Tenant", "Primary tenant", Guid.NewGuid());

        Assert.True(tenant.IsActive);
        Assert.Null(tenant.ExpiryDate);
        Assert.Equal("MAIN_TENANT", tenant.CodeName);
    }

    [Fact]
    public void Create_WithPastExpiryDate_ThrowsValidationException()
    {
        Assert.Throws<ValidationException>(() =>
            Tenant.Create("main tenant", "Main Tenant", "Primary tenant", Guid.NewGuid(), DateTime.UtcNow.AddDays(-1)));
    }
}
