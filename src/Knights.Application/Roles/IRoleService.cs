using Knights.Application.Roles.Requests;
using Knights.Application.Roles.Responses;

namespace Knights.Application.Roles;

public interface IRoleService
{
    Task<RoleResponse> CreateAsync(CreateRoleRequest request, CancellationToken cancellationToken = default);
    Task<RoleResponse?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default);
    Task<IReadOnlyCollection<RoleResponse>> GetAllAsync(CancellationToken cancellationToken = default);
    Task<RoleResponse> UpdateAsync(Guid id, UpdateRoleRequest request, CancellationToken cancellationToken = default);
    Task DeleteAsync(Guid id, CancellationToken cancellationToken = default);
    Task<RoleWithPermissionsResponse> AssignPermissionAsync(Guid id, Guid permissionId, CancellationToken cancellationToken = default);
    Task<RoleWithPermissionsResponse> RemovePermissionAsync(Guid id, Guid permissionId, CancellationToken cancellationToken = default);
}
