using Mapster;
using Knights.Application.Common.Interfaces;
using Knights.Application.Common.Mapping;
using Knights.Application.Roles.Requests;
using Knights.Application.Roles.Responses;
using Knights.Domain.Identity;

namespace Knights.Application.Roles;

public sealed class RoleService(IRoleRepository roleRepository) : IRoleService
{
    static RoleService()
    {
        MapsterConfig.Register();
    }

    public async Task<RoleResponse> CreateAsync(CreateRoleRequest request, CancellationToken cancellationToken = default)
    {
        var role = Role.Create(request.Name, request.Description, request.IsStatic, request.IsDefault, request.IsActive);
        await roleRepository.AddAsync(role, cancellationToken);
        return role.Adapt<RoleResponse>();
    }

    public async Task<RoleResponse?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default)
    {
        var role = await roleRepository.GetByIdAsync(id, cancellationToken);
        return role?.Adapt<RoleResponse>();
    }

    public async Task<IReadOnlyCollection<RoleResponse>> GetAllAsync(CancellationToken cancellationToken = default)
    {
        var roles = await roleRepository.GetAllAsync(cancellationToken);
        return roles.Select(r => r.Adapt<RoleResponse>()).ToArray();
    }

    public async Task<RoleResponse> UpdateAsync(Guid id, UpdateRoleRequest request, CancellationToken cancellationToken = default)
    {
        var role = await GetRequiredRoleAsync(id, cancellationToken);
        role.Update(request.Name, request.Description);
        await roleRepository.UpdateAsync(role, cancellationToken);
        return role.Adapt<RoleResponse>();
    }

    public async Task DeleteAsync(Guid id, CancellationToken cancellationToken = default)
    {
        await roleRepository.DeleteAsync(id, cancellationToken);
    }

    public async Task<RoleWithPermissionsResponse> AssignPermissionAsync(Guid id, Guid permissionId, CancellationToken cancellationToken = default)
    {
        var role = await GetRequiredRoleWithPermissionsAsync(id, cancellationToken);
        role.AssignPermission(permissionId);
        await roleRepository.UpdateAsync(role, cancellationToken);
        return role.Adapt<RoleWithPermissionsResponse>();
    }

    public async Task<RoleWithPermissionsResponse> RemovePermissionAsync(Guid id, Guid permissionId, CancellationToken cancellationToken = default)
    {
        var role = await GetRequiredRoleWithPermissionsAsync(id, cancellationToken);
        role.RemovePermission(permissionId);
        await roleRepository.UpdateAsync(role, cancellationToken);
        return role.Adapt<RoleWithPermissionsResponse>();
    }

    private async Task<Role> GetRequiredRoleAsync(Guid id, CancellationToken cancellationToken)
    {
        var role = await roleRepository.GetByIdAsync(id, cancellationToken);
        return role ?? throw new InvalidOperationException($"Role '{id}' was not found.");
    }

    private async Task<Role> GetRequiredRoleWithPermissionsAsync(Guid id, CancellationToken cancellationToken)
    {
        var role = await roleRepository.GetWithPermissionsAsync(id, cancellationToken);
        return role ?? throw new InvalidOperationException($"Role '{id}' was not found.");
    }
}
