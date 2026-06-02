using Knights.Application.Common.Interfaces;
using Knights.Domain.Tenants;

namespace Knights.Application.Tests.Fakes;

public sealed class InMemoryTenantRepository : ITenantRepository
{
    private readonly Dictionary<Guid, Tenant> _tenants = [];
    private readonly Dictionary<Guid, FeatureCatalogItem> _catalog = [];

    public Task<IReadOnlyCollection<Tenant>> GetAllAsync(CancellationToken ct = default)
    {
        IReadOnlyCollection<Tenant> result = _tenants.Values.OrderBy(t => t.Name).ToList();
        return Task.FromResult(result);
    }

    public Task<Tenant?> GetByIdAsync(Guid id, CancellationToken ct = default)
    {
        _tenants.TryGetValue(id, out var tenant);
        return Task.FromResult(tenant);
    }

    public Task<Tenant?> GetByCodeNameAsync(string codeName, CancellationToken ct = default)
    {
        var match = _tenants.Values.FirstOrDefault(
            t => string.Equals(t.CodeName, codeName.Trim(), StringComparison.OrdinalIgnoreCase));
        return Task.FromResult(match);
    }

    public Task<IReadOnlyCollection<FeatureCatalogItem>> GetCatalogFeaturesAsync(bool includeUnpublished, CancellationToken ct = default)
    {
        IReadOnlyCollection<FeatureCatalogItem> result = _catalog.Values
            .Where(item => includeUnpublished || (item.IsPublished && !item.IsRetired))
            .OrderBy(item => item.DisplayOrder)
            .ThenBy(item => item.Name)
            .ToList();
        return Task.FromResult(result);
    }

    public Task<FeatureCatalogItem?> GetCatalogFeatureByIdAsync(Guid id, CancellationToken ct = default)
    {
        _catalog.TryGetValue(id, out var feature);
        return Task.FromResult(feature);
    }

    public Task<FeatureCatalogItem?> GetCatalogFeatureByKeyAsync(string key, CancellationToken ct = default)
    {
        var match = _catalog.Values.FirstOrDefault(item => string.Equals(item.Key, key.Trim(), StringComparison.OrdinalIgnoreCase));
        return Task.FromResult(match);
    }

    public Task AddAsync(Tenant tenant, CancellationToken ct = default)
    {
        _tenants[tenant.Id] = tenant;
        return Task.CompletedTask;
    }

    public Task UpdateAsync(Tenant tenant, CancellationToken ct = default)
    {
        _tenants[tenant.Id] = tenant;
        return Task.CompletedTask;
    }

    public Task AddCatalogFeatureAsync(FeatureCatalogItem feature, CancellationToken ct = default)
    {
        _catalog[feature.Id] = feature;
        return Task.CompletedTask;
    }

    public Task UpdateCatalogFeatureAsync(FeatureCatalogItem feature, CancellationToken ct = default)
    {
        _catalog[feature.Id] = feature;
        return Task.CompletedTask;
    }
}
