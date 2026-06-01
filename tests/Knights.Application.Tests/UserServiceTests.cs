using Knights.Application.Tests.Fakes;
using Knights.Application.Users;
using Knights.Application.Users.Requests;
using Knights.Domain.Identity;

namespace Knights.Application.Tests;

public class UserServiceTests
{
    private static UserService CreateService(InMemoryUserRepository repository)
        => new(repository, new FakePasswordHasher());

    [Fact]
    public async Task CreateAsync_CreatesUserAndMapsResponse()
    {
        var repository = new InMemoryUserRepository();
        var service = CreateService(repository);

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
    public async Task CreateAsync_HashesPassword_DoesNotStorePlaintext()
    {
        var repository = new InMemoryUserRepository();
        var service = CreateService(repository);

        var response = await service.CreateAsync(new CreateUserRequest(
            "Ahmed",
            "Hady",
            "Ali",
            "ahmed",
            "ahmed@example.com",
            Password: "secret-password"));

        var stored = await repository.GetByIdAsync(response.Id);
        Assert.NotNull(stored);
        Assert.NotNull(stored!.PasswordHash);
        Assert.NotEqual("secret-password", stored.PasswordHash);
        Assert.True(new FakePasswordHasher().Verify("secret-password", stored.PasswordHash!));
    }

    [Fact]
    public async Task CreateAsync_WithoutPassword_LeavesHashNull()
    {
        var repository = new InMemoryUserRepository();
        var service = CreateService(repository);

        var response = await service.CreateAsync(new CreateUserRequest(
            "Ahmed", "Hady", "Ali", "ahmed", "ahmed@example.com"));

        var stored = await repository.GetByIdAsync(response.Id);
        Assert.Null(stored!.PasswordHash);
    }

    [Fact]
    public async Task GetAllAsync_ReturnsUsersForSelection()
    {
        var repository = new InMemoryUserRepository();
        var service = CreateService(repository);
        await repository.AddAsync(User.Create("Zed", "M", "Admin", "zed", "zed@example.com"));
        await repository.AddAsync(User.Create("Ada", "M", "Admin", "ada", "ada@example.com"));

        var response = await service.GetAllAsync();

        Assert.Equal(["ada", "zed"], response.Select(user => user.UserName));
    }

    [Fact]
    public async Task GetAllAsync_IncludesTenantIdForAssignedUsers()
    {
        var repository = new InMemoryUserRepository();
        var service = CreateService(repository);
        var tenantId = Guid.NewGuid();
        var user = User.Create("Ada", "M", "Admin", "ada", "ada@example.com");
        user.JoinTenant(tenantId);
        await repository.AddAsync(user);

        var response = await service.GetAllAsync();

        Assert.Equal(tenantId, response.Single().TenantId);
    }

    [Fact]
    public async Task AssignRoleAsync_UsesDomainBehaviorAndMapsRoleIds()
    {
        var repository = new InMemoryUserRepository();
        var service = CreateService(repository);
        var user = User.Create("Ahmed", "Hady", "Ali", "ahmed", "ahmed@example.com");
        await repository.AddAsync(user);
        var roleId = Guid.NewGuid();

        var response = await service.AssignRoleAsync(user.Id, roleId);

        Assert.Single(response.RoleIds);
        Assert.Equal(roleId, response.RoleIds.Single());
    }
}
