using Knights.Application.Common.Interfaces;
using Knights.Domain.Dashboards;
using Microsoft.EntityFrameworkCore;

namespace Knights.Infrastructure.Persistence.Repositories;

public sealed class DashboardRepository(KnightsDbContext dbContext) : IDashboardRepository
{
    public async Task<IReadOnlyCollection<Dashboard>> GetOwnedAsync(Guid ownerUserId, CancellationToken cancellationToken = default) =>
        await dbContext.Dashboards.Include(dashboard => dashboard.Widgets).Where(dashboard => dashboard.OwnerUserId == ownerUserId).OrderBy(dashboard => dashboard.Name).ToListAsync(cancellationToken);

    public Task<Dashboard?> GetOwnedByIdAsync(Guid id, Guid ownerUserId, CancellationToken cancellationToken = default) =>
        dbContext.Dashboards.Include(dashboard => dashboard.Widgets).FirstOrDefaultAsync(dashboard => dashboard.Id == id && dashboard.OwnerUserId == ownerUserId, cancellationToken);

    public async Task AddAsync(Dashboard dashboard, CancellationToken cancellationToken = default)
    {
        await dbContext.Dashboards.AddAsync(dashboard, cancellationToken);
        await dbContext.SaveChangesAsync(cancellationToken);
    }

    public async Task SaveAsync(CancellationToken cancellationToken = default)
    {
        await dbContext.SaveChangesAsync(cancellationToken);
    }

    public async Task DeleteAsync(Dashboard dashboard, string deletedBy, CancellationToken cancellationToken = default)
    {
        dashboard.MarkAsDeleted(deletedBy);
        await dbContext.SaveChangesAsync(cancellationToken);
    }
}
