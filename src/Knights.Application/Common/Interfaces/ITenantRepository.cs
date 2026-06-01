using Knights.Domain.Tenants;

namespace Knights.Application.Common.Interfaces;

public interface ITenantRepository
{
    Task<IReadOnlyCollection<Tenant>> GetAllAsync(CancellationToken ct = default);
    Task<Tenant?> GetByIdAsync(Guid id, CancellationToken ct = default);
    Task<Tenant?> GetByCodeNameAsync(string codeName, CancellationToken ct = default);
    Task AddAsync(Tenant tenant, CancellationToken ct = default);
    Task UpdateAsync(Tenant tenant, CancellationToken ct = default);
}
