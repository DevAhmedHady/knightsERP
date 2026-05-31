using Knights.Domain.Identity;

namespace Knights.Domain.Tests;

public class RoleTests
{
    [Fact]
    public void AssignPermission_WhenAlreadyAssigned_DoesNotDuplicate()
    {
        var role = Role.Create("Admin", "Administrator role", isStatic: false, isDefault: false);
        var permissionId = Guid.NewGuid();

        role.AssignPermission(permissionId);
        role.AssignPermission(permissionId);

        Assert.Single(role.Permissions);
        Assert.Equal(permissionId, role.Permissions.Single().PermissionId);
    }

    [Fact]
    public void RemovePermission_WhenPresent_RemovesIt()
    {
        var role = Role.Create("Admin", "Administrator role", isStatic: false, isDefault: false);
        var permissionId = Guid.NewGuid();
        role.AssignPermission(permissionId);

        role.RemovePermission(permissionId);

        Assert.Empty(role.Permissions);
    }

    [Fact]
    public void RemovePermission_WhenAbsent_IsNoOp()
    {
        var role = Role.Create("Admin", "Administrator role", isStatic: false, isDefault: false);

        var exception = Record.Exception(() => role.RemovePermission(Guid.NewGuid()));

        Assert.Null(exception);
        Assert.Empty(role.Permissions);
    }

    [Fact]
    public void Create_GeneratesCodeNameFromName()
    {
        var role = Role.Create("Super Admin", "Has all access", isStatic: true, isDefault: false);

        Assert.Equal("SUPER_ADMIN", role.CodeName);
    }
}
