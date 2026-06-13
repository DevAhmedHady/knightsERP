using Mapster;
using Knights.Application.Common.Interfaces;
using Knights.Application.Common.Mapping;
using Knights.Application.Users.Requests;
using Knights.Application.Users.Responses;
using Knights.Domain.Identity;
using Knights.Domain.Tenants;

namespace Knights.Application.Users;

public sealed class UserService(
    IUserRepository userRepository,
    IPasswordHasher passwordHasher,
    ITenantContext tenantContext,
    ITenantRepository tenantRepository) : IUserService
{
    static UserService()
    {
        MapsterConfig.Register();
    }

    public async Task<UserResponse> CreateAsync(CreateUserRequest request, CancellationToken cancellationToken = default)
    {
        var passwordHash = string.IsNullOrWhiteSpace(request.Password)
            ? null
            : passwordHasher.Hash(request.Password);

        var user = User.Create(
            request.FirstName,
            request.MidName,
            request.LastName,
            request.UserName,
            request.Email,
            passwordHash,
            request.IsEmailConfirmed);

        var currentTenant = await GetCurrentTenantAsync(cancellationToken);
        if (currentTenant is not null)
            user.JoinTenant(currentTenant.Id);

        ValidateSessionTimeoutMinutes(currentTenant, request.SessionTimeoutMinutes);
        user.SetSessionTimeoutMinutes(request.SessionTimeoutMinutes);

        await userRepository.AddAsync(user, cancellationToken);
        return user.Adapt<UserResponse>();
    }

    public async Task<UserResponse?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default)
    {
        var user = await userRepository.GetByIdAsync(id, cancellationToken);
        return user?.Adapt<UserResponse>();
    }

    public async Task<IReadOnlyCollection<UserResponse>> GetAllAsync(CancellationToken cancellationToken = default)
    {
        var users = await userRepository.GetAllAsync(cancellationToken);
        return users.Adapt<IReadOnlyCollection<UserResponse>>();
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

        var targetTenant = await GetTenantForUserAsync(user, cancellationToken);
        EnsureUserWritableByCurrentScope(targetTenant, user);
        ValidateSessionTimeoutMinutes(targetTenant, request.SessionTimeoutMinutes);
        user.SetSessionTimeoutMinutes(request.SessionTimeoutMinutes);

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

    public async Task DeleteAsync(Guid id, CancellationToken cancellationToken = default)
    {
        await userRepository.DeleteAsync(id, cancellationToken);
    }

    private async Task<User> GetRequiredUserAsync(Guid id, CancellationToken cancellationToken)
    {
        var user = await userRepository.GetByIdAsync(id, cancellationToken);
        return user ?? throw new InvalidOperationException($"User '{id}' was not found.");
    }

    private async Task<Tenant?> GetCurrentTenantAsync(CancellationToken cancellationToken)
    {
        if (!tenantContext.TenantId.HasValue)
            return null;

        return await tenantRepository.GetByIdAsync(tenantContext.TenantId.Value, cancellationToken)
            ?? throw new InvalidOperationException($"Tenant '{tenantContext.TenantId.Value}' was not found.");
    }

    private async Task<Tenant?> GetTenantForUserAsync(User user, CancellationToken cancellationToken)
    {
        if (!user.TenantId.HasValue)
            return null;

        return await tenantRepository.GetByIdAsync(user.TenantId.Value, cancellationToken)
            ?? throw new InvalidOperationException($"Tenant '{user.TenantId.Value}' was not found.");
    }

    private void EnsureUserWritableByCurrentScope(Tenant? tenant, User user)
    {
        if (!tenantContext.TenantId.HasValue)
            return;

        if (tenant is null || user.TenantId != tenantContext.TenantId)
            throw new InvalidOperationException("Tenant admins can only manage users inside their tenant.");
    }

    private static void ValidateSessionTimeoutMinutes(Tenant? tenant, int? sessionTimeoutMinutes)
    {
        if (!sessionTimeoutMinutes.HasValue || tenant is null)
            return;

        if (sessionTimeoutMinutes.Value > tenant.SessionTimeoutMinutes)
            throw new InvalidOperationException($"User session timeout cannot exceed tenant limit of {tenant.SessionTimeoutMinutes} minutes.");
    }
}
