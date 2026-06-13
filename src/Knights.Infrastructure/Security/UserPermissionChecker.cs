using Knights.Application.Common.Interfaces;
using Knights.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;

namespace Knights.Infrastructure.Security;

public sealed class UserPermissionChecker(
    KnightsDbContext dbContext,
    IUserContext userContext) : IUserPermissionChecker
{
    public async Task<bool> HasPermissionAsync(string permissionCode, CancellationToken cancellationToken = default)
    {
        if (!userContext.TenantId.HasValue)
            return true;

        var directPermissionIds = dbContext.UserPermissions
            .Where(link => !link.IsDeleted && link.UserId == userContext.UserId)
            .Select(link => link.PermissionId);

        var roleIds = from link in dbContext.UserRoles
                      join role in dbContext.Roles on link.RoleId equals role.Id
                      where !link.IsDeleted && link.UserId == userContext.UserId && !role.IsDeleted && role.IsActive
                      select link.RoleId;

        var rolePermissionIds = dbContext.RolePermissions
            .Where(link => !link.IsDeleted && roleIds.Contains(link.RoleId))
            .Select(link => link.PermissionId);

        return await dbContext.Permissions.AnyAsync(
            permission => !permission.IsDeleted
                && permission.CodeName == permissionCode
                && (directPermissionIds.Contains(permission.Id) || rolePermissionIds.Contains(permission.Id)),
            cancellationToken);
    }
}
