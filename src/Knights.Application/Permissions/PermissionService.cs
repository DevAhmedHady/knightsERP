using Mapster;
using Knights.Application.Common.Interfaces;
using Knights.Application.Common.Mapping;
using Knights.Application.Permissions.Requests;
using Knights.Application.Permissions.Responses;
using Knights.Domain.Identity;

namespace Knights.Application.Permissions;

public sealed class PermissionService(IPermissionRepository permissionRepository) : IPermissionService
{
    static PermissionService()
    {
        MapsterConfig.Register();
    }

    public async Task<PermissionResponse> CreateAsync(CreatePermissionRequest request, CancellationToken cancellationToken = default)
    {
        var permission = Permission.Create(request.CodeName, request.DisplayName, request.Description);
        await permissionRepository.AddAsync(permission, cancellationToken);
        return permission.Adapt<PermissionResponse>();
    }

    public async Task<PermissionResponse?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default)
    {
        var permission = await permissionRepository.GetByIdAsync(id, cancellationToken);
        return permission?.Adapt<PermissionResponse>();
    }

    public async Task<IReadOnlyCollection<PermissionResponse>> GetAllAsync(CancellationToken cancellationToken = default)
    {
        var permissions = await permissionRepository.GetAllAsync(cancellationToken);
        return permissions.Select(p => p.Adapt<PermissionResponse>()).ToArray();
    }

    public async Task<PermissionResponse> UpdateAsync(Guid id, UpdatePermissionRequest request, CancellationToken cancellationToken = default)
    {
        var permission = await GetRequiredPermissionAsync(id, cancellationToken);
        permission.Update(request.DisplayName, request.Description);
        await permissionRepository.UpdateAsync(permission, cancellationToken);
        return permission.Adapt<PermissionResponse>();
    }

    public async Task DeleteAsync(Guid id, CancellationToken cancellationToken = default)
    {
        await permissionRepository.DeleteAsync(id, cancellationToken);
    }

    private async Task<Permission> GetRequiredPermissionAsync(Guid id, CancellationToken cancellationToken)
    {
        var permission = await permissionRepository.GetByIdAsync(id, cancellationToken);
        return permission ?? throw new InvalidOperationException($"Permission '{id}' was not found.");
    }
}
