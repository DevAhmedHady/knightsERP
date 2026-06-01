using Microsoft.EntityFrameworkCore;
using Knights.Application.Common.Interfaces;
using Knights.Domain.Identity;

namespace Knights.Infrastructure.Persistence.Repositories;

public sealed class UserRepository(KnightsDbContext dbContext) : IUserRepository
{
    public async Task<IReadOnlyCollection<User>> GetAllAsync(CancellationToken cancellationToken = default)
    {
        return await dbContext.Users
            .AsNoTracking()
            .Include(user => user.UserRoles)
            .Include(user => user.UserPermissions)
            .OrderBy(user => user.UserName)
            .ToListAsync(cancellationToken);
    }

    public async Task<User?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default)
    {
        return await dbContext.Users
            .Include(user => user.UserRoles)
            .Include(user => user.UserPermissions)
            .FirstOrDefaultAsync(user => user.Id == id, cancellationToken);
    }

    public async Task<User?> GetByUserNameAsync(string userName, CancellationToken cancellationToken = default)
    {
        var normalized = userName.Trim().ToLower();
        return await dbContext.Users
            .Include(user => user.UserRoles)
            .Include(user => user.UserPermissions)
            .FirstOrDefaultAsync(user => user.UserName.ToLower() == normalized, cancellationToken);
    }

    public async Task<User?> GetByEmailAsync(string email, CancellationToken cancellationToken = default)
    {
        var normalized = email.Trim().ToLower();
        return await dbContext.Users
            .Include(user => user.UserRoles)
            .Include(user => user.UserPermissions)
            .FirstOrDefaultAsync(user => user.Email.ToLower() == normalized, cancellationToken);
    }

    public async Task AddAsync(User user, CancellationToken cancellationToken = default)
    {
        await dbContext.Users.AddAsync(user, cancellationToken);
        await dbContext.SaveChangesAsync(cancellationToken);
    }

    public async Task UpdateAsync(User user, CancellationToken cancellationToken = default)
    {
        dbContext.Users.Update(user);
        await dbContext.SaveChangesAsync(cancellationToken);
    }
}
