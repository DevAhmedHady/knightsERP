using Knights.Application.Auth;
using Knights.Application.Auth.Requests;
using Knights.Application.Tests.Fakes;
using Knights.Domain.Identity;
using Knights.Domain.Tenants;

namespace Knights.Application.Tests;

public class AuthServiceTests
{
    private readonly FakePasswordHasher _hasher = new();

    private (AuthService Service, InMemoryUserRepository UserRepo, InMemoryTenantRepository TenantRepo) CreateService()
    {
        var userRepo = new InMemoryUserRepository();
        var tenantRepo = new InMemoryTenantRepository();
        var service = new AuthService(userRepo, _hasher, new FakeJwtTokenGenerator(), tenantRepo, new FakeJwtSessionPolicy());
        return (service, userRepo, tenantRepo);
    }

    private async Task<User> SeedUserAsync(InMemoryUserRepository repo, string password = "secret", bool isActive = true)
    {
        var user = User.Create("Ahmed", "Hady", "Ali", "ahmed", "ahmed@example.com", _hasher.Hash(password));
        if (!isActive)
            user.SetActive(false);
        await repo.AddAsync(user);
        return user;
    }

    private async Task<User> SeedTenantUserAsync(InMemoryUserRepository userRepo, InMemoryTenantRepository tenantRepo, string password = "secret")
    {
        var tenant = Tenant.Create("acme", "Acme Corp", "Test tenant", Guid.NewGuid());
        await tenantRepo.AddAsync(tenant);

        var user = User.Create("Jane", "M", "Doe", "jane", "jane@example.com", _hasher.Hash(password));
        user.JoinTenant(tenant.Id);
        await userRepo.AddAsync(user);

        return user;
    }

    [Fact]
    public async Task LoginAsync_ValidUserName_ReturnsTokenAndUser()
    {
        var (service, userRepo, _) = CreateService();
        var user = await SeedUserAsync(userRepo);

        var result = await service.LoginAsync(new LoginRequest("ahmed", "secret"));

        Assert.NotNull(result);
        Assert.Equal($"token-for-{user.Id}", result!.AccessToken);
        Assert.True(result.ExpiresAtUtc > DateTime.UtcNow);
        Assert.Equal(user.Id, result.User.Id);
    }

    [Fact]
    public async Task LoginAsync_ValidEmail_ReturnsToken()
    {
        var (service, userRepo, _) = CreateService();
        await SeedUserAsync(userRepo);

        var result = await service.LoginAsync(new LoginRequest("ahmed@example.com", "secret"));

        Assert.NotNull(result);
    }

    [Fact]
    public async Task LoginAsync_UnknownUser_ReturnsNull()
    {
        var (service, _, _) = CreateService();

        var result = await service.LoginAsync(new LoginRequest("nobody", "secret"));

        Assert.Null(result);
    }

    [Fact]
    public async Task LoginAsync_WrongPassword_ReturnsNull()
    {
        var (service, userRepo, _) = CreateService();
        await SeedUserAsync(userRepo);

        var result = await service.LoginAsync(new LoginRequest("ahmed", "wrong"));

        Assert.Null(result);
    }

    [Fact]
    public async Task LoginAsync_InactiveUser_ReturnsNull()
    {
        var (service, userRepo, _) = CreateService();
        await SeedUserAsync(userRepo, isActive: false);

        var result = await service.LoginAsync(new LoginRequest("ahmed", "secret"));

        Assert.Null(result);
    }

    [Fact]
    public async Task LoginAsync_Success_RecordsLoginAndPersists()
    {
        var (service, userRepo, _) = CreateService();
        await SeedUserAsync(userRepo);

        await service.LoginAsync(new LoginRequest("ahmed", "secret"));

        Assert.Equal(1, userRepo.UpdateCount);
        var stored = await userRepo.GetByUserNameAsync("ahmed");
        Assert.NotNull(stored!.LastLoginDate);
    }

    [Fact]
    public async Task LoginAsync_EmptyCredentials_ReturnsNull()
    {
        var (service, _, _) = CreateService();

        Assert.Null(await service.LoginAsync(new LoginRequest("", "secret")));
        Assert.Null(await service.LoginAsync(new LoginRequest("ahmed", "")));
    }

    [Fact]
    public async Task LoginAsync_SystemAdmin_SucceedsWithoutTenantCodeName()
    {
        var (service, userRepo, _) = CreateService();
        var user = await SeedUserAsync(userRepo);
        Assert.True(user.IsSystemAdmin);

        var result = await service.LoginAsync(new LoginRequest("ahmed", "secret", null));

        Assert.NotNull(result);
        Assert.Null(result!.TenantId);
        Assert.Null(result.TenantCodeName);
    }

    [Fact]
    public async Task LoginAsync_TenantUser_WithCorrectTenantCodeName_Succeeds()
    {
        var (service, userRepo, tenantRepo) = CreateService();
        await SeedTenantUserAsync(userRepo, tenantRepo);

        var result = await service.LoginAsync(new LoginRequest("jane", "secret", "ACME"));

        Assert.NotNull(result);
        Assert.NotNull(result!.TenantId);
        Assert.Equal("ACME", result.TenantCodeName);
    }

    [Fact]
    public async Task LoginAsync_TenantUser_WithWrongTenantCodeName_ReturnsNull()
    {
        var (service, userRepo, tenantRepo) = CreateService();
        await SeedTenantUserAsync(userRepo, tenantRepo);

        var result = await service.LoginAsync(new LoginRequest("jane", "secret", "WRONG_TENANT"));

        Assert.Null(result);
    }

    [Fact]
    public async Task LoginAsync_TenantUser_WithMissingTenantCodeName_ReturnsNull()
    {
        var (service, userRepo, tenantRepo) = CreateService();
        await SeedTenantUserAsync(userRepo, tenantRepo);

        var result = await service.LoginAsync(new LoginRequest("jane", "secret", null));

        Assert.Null(result);
    }

    [Fact]
    public async Task LoginAsync_InactiveTenant_ReturnsNull()
    {
        var (service, userRepo, tenantRepo) = CreateService();
        var tenant = Tenant.Create("acme", "Acme Corp", "desc", Guid.NewGuid());
        tenant.SetActive(false);
        await tenantRepo.AddAsync(tenant);

        var user = User.Create("Jane", "M", "Doe", "jane", "jane@example.com", _hasher.Hash("secret"));
        user.JoinTenant(tenant.Id);
        await userRepo.AddAsync(user);

        var result = await service.LoginAsync(new LoginRequest("jane", "secret", "ACME"));

        Assert.Null(result);
    }

    [Fact]
    public async Task LoginAsync_ExpiredTenant_ReturnsNull()
    {
        var (service, userRepo, tenantRepo) = CreateService();
        var tenant = Tenant.Create("acme", "Acme Corp", "desc", Guid.NewGuid());

        // Simulate expiry by setting ExpiryDate to the past via reflection
        typeof(Tenant)
            .GetProperty(nameof(Tenant.ExpiryDate))!
            .SetValue(tenant, DateTime.UtcNow.AddDays(-1));

        await tenantRepo.AddAsync(tenant);

        var user = User.Create("Jane", "M", "Doe", "jane", "jane@example.com", _hasher.Hash("secret"));
        user.JoinTenant(tenant.Id);
        await userRepo.AddAsync(user);

        var result = await service.LoginAsync(new LoginRequest("jane", "secret", "ACME"));

        Assert.Null(result);
    }
}
