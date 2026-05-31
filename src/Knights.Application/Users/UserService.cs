using Mapster;
using Knights.Application.Common.Interfaces;
using Knights.Application.Common.Mapping;
using Knights.Application.Users.Requests;
using Knights.Application.Users.Responses;
using Knights.Domain.Identity;

namespace Knights.Application.Users;

public sealed class UserService(IUserRepository userRepository) : IUserService
{
    static UserService()
    {
        MapsterConfig.Register();
    }

    public async Task<UserResponse> CreateAsync(CreateUserRequest request, CancellationToken cancellationToken = default)
    {
        var user = User.Create(
            request.FirstName,
            request.MidName,
            request.LastName,
            request.UserName,
            request.Email,
            request.PasswordHash,
            request.IsEmailConfirmed);

        await userRepository.AddAsync(user, cancellationToken);
        return user.Adapt<UserResponse>();
    }

    public async Task<UserResponse?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default)
    {
        var user = await userRepository.GetByIdAsync(id, cancellationToken);
        return user?.Adapt<UserResponse>();
    }

    public async Task<UserResponse> UpdateAsync(Guid id, UpdateUserRequest request, CancellationToken cancellationToken = default)
    {
        var user = await GetRequiredUserAsync(id, cancellationToken);

        user.UpdateProfile(
            request.FirstName,
            request.MidName,
            request.LastName,
            request.UserName,
            request.Email,
            request.IsEmailConfirmed);

        await userRepository.UpdateAsync(user, cancellationToken);
        return user.Adapt<UserResponse>();
    }

    public async Task<UserResponse> AssignRoleAsync(Guid id, Guid roleId, CancellationToken cancellationToken = default)
    {
        var user = await GetRequiredUserAsync(id, cancellationToken);
        user.AssignRole(roleId);

        await userRepository.UpdateAsync(user, cancellationToken);
        return user.Adapt<UserResponse>();
    }

    public async Task<UserResponse> GrantPermissionAsync(Guid id, Guid permissionId, CancellationToken cancellationToken = default)
    {
        var user = await GetRequiredUserAsync(id, cancellationToken);
        user.GrantPermission(permissionId);

        await userRepository.UpdateAsync(user, cancellationToken);
        return user.Adapt<UserResponse>();
    }

    private async Task<User> GetRequiredUserAsync(Guid id, CancellationToken cancellationToken)
    {
        var user = await userRepository.GetByIdAsync(id, cancellationToken);
        return user ?? throw new InvalidOperationException($"User '{id}' was not found.");
    }
}
