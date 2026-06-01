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

    [Fact]
    public void AssignRole_WhenRoleAlreadyAssigned_DoesNotDuplicate()
    {
        var tenant = Tenant.Create("acme", "Acme Corp", "desc", Guid.NewGuid());
        var roleId = Guid.NewGuid();

        tenant.AssignRole(roleId);
        tenant.AssignRole(roleId);

        Assert.Single(tenant.TenantRoles);
    }

    [Fact]
    public void GrantPermission_AddsPermission()
    {
        var tenant = Tenant.Create("acme", "Acme Corp", "desc", Guid.NewGuid());
        var permissionId = Guid.NewGuid();

        tenant.GrantPermission(permissionId);

        Assert.Single(tenant.TenantPermissions);
        Assert.Equal(permissionId, tenant.TenantPermissions.Single().PermissionId);
    }

    [Fact]
    public void GrantPermission_WhenAlreadyGranted_DoesNotDuplicate()
    {
        var tenant = Tenant.Create("acme", "Acme Corp", "desc", Guid.NewGuid());
        var permissionId = Guid.NewGuid();

        tenant.GrantPermission(permissionId);
        tenant.GrantPermission(permissionId);

        Assert.Single(tenant.TenantPermissions);
    }

    [Fact]
    public void RevokePermission_RemovesPermission()
    {
        var tenant = Tenant.Create("acme", "Acme Corp", "desc", Guid.NewGuid());
        var permissionId = Guid.NewGuid();

        tenant.GrantPermission(permissionId);
        tenant.RevokePermission(permissionId);

        Assert.Empty(tenant.TenantPermissions);
    }

    [Fact]
    public void IsExpired_WhenNoExpiryDate_ReturnsFalse()
    {
        var tenant = Tenant.Create("acme", "Acme Corp", "desc", Guid.NewGuid());

        Assert.False(tenant.IsExpired);
    }

    [Fact]
    public void IsExpired_WhenExpiryDateInFuture_ReturnsFalse()
    {
        var tenant = Tenant.Create("acme", "Acme Corp", "desc", Guid.NewGuid(), DateTime.UtcNow.AddDays(30));

        Assert.False(tenant.IsExpired);
    }

    [Fact]
    public void SetActive_ToFalse_DeactivatesTenant()
    {
        var tenant = Tenant.Create("acme", "Acme Corp", "desc", Guid.NewGuid());

        tenant.SetActive(false);

        Assert.False(tenant.IsActive);
    }
}
