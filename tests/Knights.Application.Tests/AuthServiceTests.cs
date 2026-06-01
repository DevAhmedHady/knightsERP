using Knights.Application.Auth;
using Knights.Application.Auth.Requests;
using Knights.Application.Tests.Fakes;
using Knights.Domain.Identity;

namespace Knights.Application.Tests;

public class AuthServiceTests
{
    private readonly FakePasswordHasher _hasher = new();

    private (AuthService Service, InMemoryUserRepository Repo) CreateService()
    {
        var repo = new InMemoryUserRepository();
        var service = new AuthService(repo, _hasher, new FakeJwtTokenGenerator());
        return (service, repo);
    }

    private async Task<User> SeedUserAsync(InMemoryUserRepository repo, string password = "secret", bool isActive = true)
    {
        var user = User.Create("Ahmed", "Hady", "Ali", "ahmed", "ahmed@example.com", _hasher.Hash(password));
        if (!isActive)
            user.SetActive(false);
        await repo.AddAsync(user);
        return user;
    }

    [Fact]
    public async Task LoginAsync_ValidUserName_ReturnsTokenAndUser()
    {
        var (service, repo) = CreateService();
        var user = await SeedUserAsync(repo);

        var result = await service.LoginAsync(new LoginRequest("ahmed", "secret"));

        Assert.NotNull(result);
        Assert.Equal($"token-for-{user.Id}", result!.AccessToken);
        Assert.True(result.ExpiresAtUtc > DateTime.UtcNow);
        Assert.Equal(user.Id, result.User.Id);
    }

    [Fact]
    public async Task LoginAsync_ValidEmail_ReturnsToken()
    {
        var (service, repo) = CreateService();
        await SeedUserAsync(repo);

        var result = await service.LoginAsync(new LoginRequest("ahmed@example.com", "secret"));

        Assert.NotNull(result);
    }

    [Fact]
    public async Task LoginAsync_UnknownUser_ReturnsNull()
    {
        var (service, _) = CreateService();

        var result = await service.LoginAsync(new LoginRequest("nobody", "secret"));

        Assert.Null(result);
    }

    [Fact]
    public async Task LoginAsync_WrongPassword_ReturnsNull()
    {
        var (service, repo) = CreateService();
        await SeedUserAsync(repo);

        var result = await service.LoginAsync(new LoginRequest("ahmed", "wrong"));

        Assert.Null(result);
    }

    [Fact]
    public async Task LoginAsync_InactiveUser_ReturnsNull()
    {
        var (service, repo) = CreateService();
        await SeedUserAsync(repo, isActive: false);

        var result = await service.LoginAsync(new LoginRequest("ahmed", "secret"));

        Assert.Null(result);
    }

    [Fact]
    public async Task LoginAsync_Success_RecordsLoginAndPersists()
    {
        var (service, repo) = CreateService();
        await SeedUserAsync(repo);

        await service.LoginAsync(new LoginRequest("ahmed", "secret"));

        Assert.Equal(1, repo.UpdateCount);
        var stored = await repo.GetByUserNameAsync("ahmed");
        Assert.NotNull(stored!.LastLoginDate);
    }

    [Fact]
    public async Task LoginAsync_EmptyCredentials_ReturnsNull()
    {
        var (service, _) = CreateService();

        Assert.Null(await service.LoginAsync(new LoginRequest("", "secret")));
        Assert.Null(await service.LoginAsync(new LoginRequest("ahmed", "")));
    }
}
