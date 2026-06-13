using Knights.Domain.Dashboards;

namespace Knights.Application.Common.Interfaces;

public interface IDashboardRepository
{
    Task<IReadOnlyCollection<Dashboard>> GetOwnedAsync(Guid ownerUserId, CancellationToken cancellationToken = default);
    Task<Dashboard?> GetOwnedByIdAsync(Guid id, Guid ownerUserId, CancellationToken cancellationToken = default);
    Task AddAsync(Dashboard dashboard, CancellationToken cancellationToken = default);
    Task SaveAsync(CancellationToken cancellationToken = default);
    Task DeleteAsync(Dashboard dashboard, string deletedBy, CancellationToken cancellationToken = default);
}
