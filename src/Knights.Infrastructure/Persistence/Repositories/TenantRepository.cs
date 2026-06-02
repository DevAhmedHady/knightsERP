using Microsoft.EntityFrameworkCore;
using Knights.Application.Common.Interfaces;
using Knights.Domain.Tenants;

namespace Knights.Infrastructure.Persistence.Repositories;

public sealed class TenantRepository(KnightsDbContext dbContext) : ITenantRepository
{
    public async Task<IReadOnlyCollection<Tenant>> GetAllAsync(CancellationToken cancellationToken = default)
    {
        return await dbContext.Tenants
            .AsNoTracking()
            .Include(t => t.TenantRoles)
            .Include(t => t.TenantPermissions)
            .Include(t => t.TenantFeatureSelections)
            .ThenInclude(selection => selection.FeatureCatalogItem)
            .OrderBy(t => t.Name)
            .ToListAsync(cancellationToken);
    }

    public async Task<Tenant?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default)
    {
        return await dbContext.Tenants
            .Include(t => t.TenantRoles)
            .Include(t => t.TenantPermissions)
            .Include(t => t.TenantFeatureSelections)
            .ThenInclude(selection => selection.FeatureCatalogItem)
            .FirstOrDefaultAsync(t => t.Id == id, cancellationToken);
    }

    public async Task<Tenant?> GetByCodeNameAsync(string codeName, CancellationToken cancellationToken = default)
    {
        var normalized = codeName.Trim().ToUpperInvariant();
        return await dbContext.Tenants
            .Include(t => t.TenantRoles)
            .Include(t => t.TenantPermissions)
            .Include(t => t.TenantFeatureSelections)
            .ThenInclude(selection => selection.FeatureCatalogItem)
            .FirstOrDefaultAsync(t => t.CodeName == normalized, cancellationToken);
    }

    public async Task<IReadOnlyCollection<FeatureCatalogItem>> GetCatalogFeaturesAsync(bool includeUnpublished, CancellationToken cancellationToken = default)
    {
        var query = dbContext.FeatureCatalogItems.AsNoTracking().OrderBy(item => item.DisplayOrder).ThenBy(item => item.Name);
        if (!includeUnpublished)
            query = query.Where(item => item.IsPublished && !item.IsRetired).OrderBy(item => item.DisplayOrder).ThenBy(item => item.Name);

        return await query.ToListAsync(cancellationToken);
    }

    public Task<FeatureCatalogItem?> GetCatalogFeatureByIdAsync(Guid id, CancellationToken cancellationToken = default)
    {
        return dbContext.FeatureCatalogItems.FirstOrDefaultAsync(item => item.Id == id, cancellationToken);
    }

    public Task<FeatureCatalogItem?> GetCatalogFeatureByKeyAsync(string key, CancellationToken cancellationToken = default)
    {
        var normalized = key.Trim().ToUpperInvariant();
        return dbContext.FeatureCatalogItems.FirstOrDefaultAsync(item => item.Key == normalized, cancellationToken);
    }

    public async Task AddAsync(Tenant tenant, CancellationToken cancellationToken = default)
    {
        await dbContext.Tenants.AddAsync(tenant, cancellationToken);
        await dbContext.SaveChangesAsync(cancellationToken);
    }

    public async Task UpdateAsync(Tenant tenant, CancellationToken cancellationToken = default)
    {
        dbContext.Tenants.Update(tenant);
        await dbContext.SaveChangesAsync(cancellationToken);
    }

    public async Task AddCatalogFeatureAsync(FeatureCatalogItem feature, CancellationToken cancellationToken = default)
    {
        await dbContext.FeatureCatalogItems.AddAsync(feature, cancellationToken);
        await dbContext.SaveChangesAsync(cancellationToken);
    }

    public async Task UpdateCatalogFeatureAsync(FeatureCatalogItem feature, CancellationToken cancellationToken = default)
    {
        dbContext.FeatureCatalogItems.Update(feature);
        await dbContext.SaveChangesAsync(cancellationToken);
    }
}
