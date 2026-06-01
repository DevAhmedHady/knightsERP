using Mapster;
using Knights.Application.Common.Interfaces;
using Knights.Application.Common.Mapping;
using Knights.Application.Tenants.Requests;
using Knights.Application.Tenants.Responses;
using Knights.Domain.Tenants;

namespace Knights.Application.Tenants;

public sealed class TenantService(
    ITenantRepository tenantRepository,
    IUserRepository userRepository) : ITenantService
{
    static TenantService()
    {
        MapsterConfig.Register();
    }

    public async Task<TenantResponse> CreateAsync(CreateTenantRequest request, CancellationToken ct = default)
    {
        var tenant = Tenant.Create(request.CodeName, request.Name, request.Description, request.OwnerId, request.ExpiryDate);
        await tenantRepository.AddAsync(tenant, ct);
        return tenant.Adapt<TenantResponse>();
    }

    public async Task<IReadOnlyCollection<TenantResponse>> GetAllAsync(CancellationToken ct = default)
    {
        var tenants = await tenantRepository.GetAllAsync(ct);
        return tenants.Adapt<IReadOnlyCollection<TenantResponse>>();
    }

    public async Task<TenantResponse?> GetByIdAsync(Guid id, CancellationToken ct = default)
    {
        var tenant = await tenantRepository.GetByIdAsync(id, ct);
        return tenant?.Adapt<TenantResponse>();
    }

    public async Task<TenantResponse?> GetByCodeNameAsync(string codeName, CancellationToken ct = default)
    {
        var tenant = await tenantRepository.GetByCodeNameAsync(codeName, ct);
        return tenant?.Adapt<TenantResponse>();
    }

    public async Task UpdateAsync(Guid id, UpdateTenantRequest request, CancellationToken ct = default)
    {
        var tenant = await GetRequiredTenantAsync(id, ct);
        tenant.Update(request.Name, request.Description, request.ExpiryDate);
        await tenantRepository.UpdateAsync(tenant, ct);
    }

    public async Task AssignRoleAsync(Guid tenantId, Guid roleId, CancellationToken ct = default)
    {
        var tenant = await GetRequiredTenantAsync(tenantId, ct);
        tenant.AssignRole(roleId);
        await tenantRepository.UpdateAsync(tenant, ct);
    }

    public async Task RemoveRoleAsync(Guid tenantId, Guid roleId, CancellationToken ct = default)
    {
        var tenant = await GetRequiredTenantAsync(tenantId, ct);
        tenant.RemoveRole(roleId);
        await tenantRepository.UpdateAsync(tenant, ct);
    }

    public async Task GrantPermissionAsync(Guid tenantId, Guid permissionId, CancellationToken ct = default)
    {
        var tenant = await GetRequiredTenantAsync(tenantId, ct);
        tenant.GrantPermission(permissionId);
        await tenantRepository.UpdateAsync(tenant, ct);
    }

    public async Task RevokePermissionAsync(Guid tenantId, Guid permissionId, CancellationToken ct = default)
    {
        var tenant = await GetRequiredTenantAsync(tenantId, ct);
        tenant.RevokePermission(permissionId);
        await tenantRepository.UpdateAsync(tenant, ct);
    }

    public async Task AddMemberAsync(Guid tenantId, Guid userId, CancellationToken ct = default)
    {
        var tenant = await GetRequiredTenantAsync(tenantId, ct);
        if (!tenant.IsActive || tenant.IsExpired)
            throw new InvalidOperationException($"Tenant '{tenantId}' is not active.");

        var user = await userRepository.GetByIdAsync(userId, ct)
            ?? throw new InvalidOperationException($"User '{userId}' was not found.");

        if (user.TenantId.HasValue && user.TenantId.Value != tenantId)
            throw new InvalidOperationException($"User '{userId}' already belongs to a different tenant.");

        user.JoinTenant(tenantId);
        await userRepository.UpdateAsync(user, ct);
    }

    public async Task RemoveMemberAsync(Guid tenantId, Guid userId, CancellationToken ct = default)
    {
        var user = await userRepository.GetByIdAsync(userId, ct)
            ?? throw new InvalidOperationException($"User '{userId}' was not found.");

        if (user.TenantId != tenantId)
            throw new InvalidOperationException($"User '{userId}' does not belong to tenant '{tenantId}'.");

        user.LeaveTenant();
        await userRepository.UpdateAsync(user, ct);
    }

    private async Task<Tenant> GetRequiredTenantAsync(Guid id, CancellationToken ct)
    {
        var tenant = await tenantRepository.GetByIdAsync(id, ct);
        return tenant ?? throw new InvalidOperationException($"Tenant '{id}' was not found.");
    }
}
