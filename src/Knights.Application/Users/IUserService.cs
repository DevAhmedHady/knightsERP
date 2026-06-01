using Knights.Application.Users.Requests;
using Knights.Application.Users.Responses;

namespace Knights.Application.Users;

public interface IUserService
{
    Task<UserResponse> CreateAsync(CreateUserRequest request, CancellationToken cancellationToken = default);
    Task<IReadOnlyCollection<UserResponse>> GetAllAsync(CancellationToken cancellationToken = default);
    Task<UserResponse?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default);
    Task<UserResponse> UpdateAsync(Guid id, UpdateUserRequest request, CancellationToken cancellationToken = default);
    Task<UserResponse> AssignRoleAsync(Guid id, Guid roleId, CancellationToken cancellationToken = default);
    Task<UserResponse> GrantPermissionAsync(Guid id, Guid permissionId, CancellationToken cancellationToken = default);
}
