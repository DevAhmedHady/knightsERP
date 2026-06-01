using Knights.Domain.Exceptions;
using Knights.Domain.Identity;

namespace Knights.Domain.Tests;

public class UserTests
{
    [Fact]
    public void Create_WithInvalidEmail_ThrowsValidationException()
    {
        Assert.Throws<ValidationException>(() =>
            User.Create("Ahmed", "Hady", "Ali", "ahmed", "not-an-email"));
    }

    [Fact]
    public void AssignRole_WhenRoleAlreadyAssigned_DoesNotDuplicate()
    {
        var user = User.Create("Ahmed", "Hady", "Ali", "ahmed", "ahmed@example.com");
        var roleId = Guid.NewGuid();

        user.AssignRole(roleId);
        user.AssignRole(roleId);

        Assert.Single(user.UserRoles);
        Assert.Equal(roleId, user.UserRoles.Single().RoleId);
    }

    [Fact]
    public void GrantPermission_WhenPermissionAlreadyGranted_DoesNotDuplicate()
    {
        var user = User.Create("Ahmed", "Hady", "Ali", "ahmed", "ahmed@example.com");
        var permissionId = Guid.NewGuid();

        user.GrantPermission(permissionId);
        user.GrantPermission(permissionId);

        Assert.Single(user.UserPermissions);
        Assert.Equal(permissionId, user.UserPermissions.Single().PermissionId);
    }

    [Fact]
    public void Create_WithoutTenant_IsSystemAdmin()
    {
        var user = User.Create("Ahmed", "Hady", "Ali", "ahmed", "ahmed@example.com");

        Assert.Null(user.TenantId);
        Assert.True(user.IsSystemAdmin);
    }

    [Fact]
    public void JoinTenant_SetsTenantId()
    {
        var user = User.Create("Ahmed", "Hady", "Ali", "ahmed", "ahmed@example.com");
        var tenantId = Guid.NewGuid();

        user.JoinTenant(tenantId);

        Assert.Equal(tenantId, user.TenantId);
        Assert.False(user.IsSystemAdmin);
    }

    [Fact]
    public void LeaveTenant_ClearsTenantId()
    {
        var user = User.Create("Ahmed", "Hady", "Ali", "ahmed", "ahmed@example.com");
        var tenantId = Guid.NewGuid();

        user.JoinTenant(tenantId);
        user.LeaveTenant();

        Assert.Null(user.TenantId);
        Assert.True(user.IsSystemAdmin);
    }

    [Fact]
    public void JoinTenant_WithEmptyGuid_ThrowsValidationException()
    {
        var user = User.Create("Ahmed", "Hady", "Ali", "ahmed", "ahmed@example.com");

        Assert.Throws<ValidationException>(() => user.JoinTenant(Guid.Empty));
    }
}
