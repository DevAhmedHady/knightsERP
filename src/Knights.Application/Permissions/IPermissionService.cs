using Knights.Application.Permissions.Requests;
using Knights.Application.Permissions.Responses;

namespace Knights.Application.Permissions;

public interface IPermissionService
{
    Task<PermissionResponse> CreateAsync(CreatePermissionRequest request, CancellationToken cancellationToken = default);
    Task<PermissionResponse?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default);
    Task<IReadOnlyCollection<PermissionResponse>> GetAllAsync(CancellationToken cancellationToken = default);
    Task<PermissionResponse> UpdateAsync(Guid id, UpdatePermissionRequest request, CancellationToken cancellationToken = default);
    Task DeleteAsync(Guid id, CancellationToken cancellationToken = default);
}
