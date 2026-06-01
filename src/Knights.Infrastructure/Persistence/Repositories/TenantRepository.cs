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
            .OrderBy(t => t.Name)
            .ToListAsync(cancellationToken);
    }

    public async Task<Tenant?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default)
    {
        return await dbContext.Tenants
            .Include(t => t.TenantRoles)
            .Include(t => t.TenantPermissions)
            .FirstOrDefaultAsync(t => t.Id == id, cancellationToken);
    }

    public async Task<Tenant?> GetByCodeNameAsync(string codeName, CancellationToken cancellationToken = default)
    {
        var normalized = codeName.Trim().ToUpperInvariant();
        return await dbContext.Tenants
            .Include(t => t.TenantRoles)
            .Include(t => t.TenantPermissions)
            .FirstOrDefaultAsync(t => t.CodeName == normalized, cancellationToken);
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
}
