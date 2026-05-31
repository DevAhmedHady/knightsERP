using Microsoft.EntityFrameworkCore;
using Knights.Application.Common.Interfaces;
using Knights.Domain.Identity;

namespace Knights.Infrastructure.Persistence.Repositories;

public sealed class PermissionRepository(KnightsDbContext dbContext) : IPermissionRepository
{
    public async Task<Permission?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default)
    {
        return await dbContext.Permissions
            .FirstOrDefaultAsync(p => p.Id == id, cancellationToken);
    }

    public async Task<IReadOnlyCollection<Permission>> GetAllAsync(CancellationToken cancellationToken = default)
    {
        return await dbContext.Permissions.ToListAsync(cancellationToken);
    }

    public async Task<Permission?> GetByCodeNameAsync(string codeName, CancellationToken cancellationToken = default)
    {
        return await dbContext.Permissions
            .FirstOrDefaultAsync(p => p.CodeName == codeName, cancellationToken);
    }

    public async Task AddAsync(Permission permission, CancellationToken cancellationToken = default)
    {
        await dbContext.Permissions.AddAsync(permission, cancellationToken);
        await dbContext.SaveChangesAsync(cancellationToken);
    }

    public async Task UpdateAsync(Permission permission, CancellationToken cancellationToken = default)
    {
        dbContext.Permissions.Update(permission);
        await dbContext.SaveChangesAsync(cancellationToken);
    }

    public async Task DeleteAsync(Guid id, CancellationToken cancellationToken = default)
    {
        var permission = await dbContext.Permissions.FirstOrDefaultAsync(p => p.Id == id, cancellationToken);
        if (permission is null)
            return;

        permission.MarkAsDeleted("system");
        dbContext.Permissions.Update(permission);
        await dbContext.SaveChangesAsync(cancellationToken);
    }
}
