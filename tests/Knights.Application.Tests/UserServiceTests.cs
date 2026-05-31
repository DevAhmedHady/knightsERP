using Knights.Application.Common.Interfaces;
using Knights.Application.Users;
using Knights.Application.Users.Requests;
using Knights.Domain.Identity;

namespace Knights.Application.Tests;

public class UserServiceTests
{
    [Fact]
    public async Task CreateAsync_CreatesUserAndMapsResponse()
    {
        var repository = new InMemoryUserRepository();
        var service = new UserService(repository);

        var response = await service.CreateAsync(new CreateUserRequest(
            "Ahmed",
            "Hady",
            "Ali",
            "ahmed",
            "ahmed@example.com"));

        Assert.NotEqual(Guid.Empty, response.Id);
        Assert.Equal("Ahmed", response.FirstName);
        Assert.Equal("Hady", response.MidName);
        Assert.Equal("Ali", response.LastName);
        Assert.Equal("ahmed", response.UserName);
        Assert.Equal("ahmed@example.com", response.Email);
        Assert.True(response.IsActive);
    }

    [Fact]
    public async Task AssignRoleAsync_UsesDomainBehaviorAndMapsRoleIds()
    {
        var repository = new InMemoryUserRepository();
        var service = new UserService(repository);
        var user = User.Create("Ahmed", "Hady", "Ali", "ahmed", "ahmed@example.com");
        await repository.AddAsync(user);
        var roleId = Guid.NewGuid();

        var response = await service.AssignRoleAsync(user.Id, roleId);

        Assert.Single(response.RoleIds);
        Assert.Equal(roleId, response.RoleIds.Single());
    }

    private sealed class InMemoryUserRepository : IUserRepository
    {
        private readonly Dictionary<Guid, User> _users = [];

        public Task<User?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default)
        {
            _users.TryGetValue(id, out var user);
            return Task.FromResult(user);
        }

        public Task AddAsync(User user, CancellationToken cancellationToken = default)
        {
            _users[user.Id] = user;
            return Task.CompletedTask;
        }

        public Task UpdateAsync(User user, CancellationToken cancellationToken = default)
        {
            _users[user.Id] = user;
            return Task.CompletedTask;
        }
    }
}
