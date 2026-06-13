using Knights.Application.Tenants.Requests;
using Knights.Application.Tenants.Responses;

namespace Knights.Application.Tenants;

public interface ITenantService
{
    Task<TenantResponse> CreateAsync(CreateTenantRequest request, CancellationToken ct = default);
    Task<IReadOnlyCollection<TenantResponse>> GetAllAsync(CancellationToken ct = default);
    Task<TenantResponse?> GetByIdAsync(Guid id, CancellationToken ct = default);
    Task<TenantResponse?> GetByCodeNameAsync(string codeName, CancellationToken ct = default);
    Task UpdateAsync(Guid id, UpdateTenantRequest request, CancellationToken ct = default);
    Task DeactivateAsync(Guid id, CancellationToken ct = default);
    Task AssignRoleAsync(Guid tenantId, Guid roleId, CancellationToken ct = default);
    Task RemoveRoleAsync(Guid tenantId, Guid roleId, CancellationToken ct = default);
    Task GrantPermissionAsync(Guid tenantId, Guid permissionId, CancellationToken ct = default);
    Task RevokePermissionAsync(Guid tenantId, Guid permissionId, CancellationToken ct = default);
    Task AddMemberAsync(Guid tenantId, Guid userId, CancellationToken ct = default);
    Task RemoveMemberAsync(Guid tenantId, Guid userId, CancellationToken ct = default);
    Task<TenantSetupSummaryResponse> GetCurrentSetupAsync(CancellationToken ct = default);
    Task<TenantSetupSummaryResponse> ConfigureCurrentEnvironmentAsync(ConfigureTenantEnvironmentRequest request, CancellationToken ct = default);
    Task<TenantSetupSummaryResponse> SelectCurrentFeatureAsync(Guid featureId, CancellationToken ct = default);
    Task<TenantSetupSummaryResponse> RemoveCurrentFeatureAsync(Guid featureId, CancellationToken ct = default);
    Task<TenantSetupSummaryResponse> UpdateCurrentFeatureSettingsAsync(Guid featureId, UpdateTenantFeatureSettingsRequest request, CancellationToken ct = default);
    Task<IReadOnlyCollection<FeatureCatalogItemResponse>> GetCatalogAsync(CancellationToken ct = default);
    Task<FeatureCatalogItemResponse> CreateCatalogFeatureAsync(CreateFeatureCatalogItemRequest request, CancellationToken ct = default);
    Task<FeatureCatalogItemResponse> UpdateCatalogFeatureAsync(Guid featureId, UpdateFeatureCatalogItemRequest request, CancellationToken ct = default);
}
