using Knights.Application.Common.Interfaces;
using Knights.Domain.Identity;

namespace Knights.Application.Tests.Fakes;

public sealed class InMemoryUserRepository : IUserRepository
{
    private readonly Dictionary<Guid, User> _users = [];

    public int UpdateCount { get; private set; }

    public Task<IReadOnlyCollection<User>> GetAllAsync(CancellationToken cancellationToken = default)
    {
        return Task.FromResult<IReadOnlyCollection<User>>(
            _users.Values.OrderBy(user => user.UserName).ToList());
    }

    public Task<User?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default)
    {
        _users.TryGetValue(id, out var user);
        return Task.FromResult(user);
    }

    public Task<User?> GetByUserNameAsync(string userName, CancellationToken cancellationToken = default)
    {
        var match = _users.Values.FirstOrDefault(
            user => string.Equals(user.UserName, userName.Trim(), StringComparison.OrdinalIgnoreCase));
        return Task.FromResult(match);
    }

    public Task<User?> GetByEmailAsync(string email, CancellationToken cancellationToken = default)
    {
        var match = _users.Values.FirstOrDefault(
            user => string.Equals(user.Email, email.Trim(), StringComparison.OrdinalIgnoreCase));
        return Task.FromResult(match);
    }

    public Task AddAsync(User user, CancellationToken cancellationToken = default)
    {
        _users[user.Id] = user;
        return Task.CompletedTask;
    }

    public Task UpdateAsync(User user, CancellationToken cancellationToken = default)
    {
        _users[user.Id] = user;
        UpdateCount++;
        return Task.CompletedTask;
    }

    public Task DeleteAsync(Guid id, CancellationToken cancellationToken = default)
    {
        _users.Remove(id);
        return Task.CompletedTask;
    }
}
