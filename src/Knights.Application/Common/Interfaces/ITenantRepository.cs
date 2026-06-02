using Knights.Domain.Tenants;

namespace Knights.Application.Common.Interfaces;

public interface ITenantRepository
{
    Task<IReadOnlyCollection<Tenant>> GetAllAsync(CancellationToken ct = default);
    Task<Tenant?> GetByIdAsync(Guid id, CancellationToken ct = default);
    Task<Tenant?> GetByCodeNameAsync(string codeName, CancellationToken ct = default);
    Task<IReadOnlyCollection<FeatureCatalogItem>> GetCatalogFeaturesAsync(bool includeUnpublished, CancellationToken ct = default);
    Task<FeatureCatalogItem?> GetCatalogFeatureByIdAsync(Guid id, CancellationToken ct = default);
    Task<FeatureCatalogItem?> GetCatalogFeatureByKeyAsync(string key, CancellationToken ct = default);
    Task AddAsync(Tenant tenant, CancellationToken ct = default);
    Task UpdateAsync(Tenant tenant, CancellationToken ct = default);
    Task AddCatalogFeatureAsync(FeatureCatalogItem feature, CancellationToken ct = default);
    Task UpdateCatalogFeatureAsync(FeatureCatalogItem feature, CancellationToken ct = default);
}
