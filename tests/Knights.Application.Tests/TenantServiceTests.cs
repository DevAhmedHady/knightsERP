using Knights.Application.Tenants;
using Knights.Application.Tenants.Requests;
using Knights.Application.Tests.Fakes;
using Knights.Domain.Identity;

namespace Knights.Application.Tests;

public class TenantServiceTests
{
    private readonly FakePasswordHasher _hasher = new();

    private (TenantService Service, InMemoryTenantRepository TenantRepo, InMemoryUserRepository UserRepo) CreateService()
    {
        var tenantRepo = new InMemoryTenantRepository();
        var userRepo = new InMemoryUserRepository();
        var service = new TenantService(tenantRepo, userRepo);
        return (service, tenantRepo, userRepo);
    }

    private static CreateTenantRequest MakeCreateRequest(string codeName = "acme") =>
        new(codeName, "Acme Corp", "Test tenant", Guid.NewGuid(), null);

    [Fact]
    public async Task CreateAsync_CreatesTenantAndStoresIt()
    {
        var (service, tenantRepo, _) = CreateService();
        var request = MakeCreateRequest();

        var response = await service.CreateAsync(request);

        Assert.NotEqual(Guid.Empty, response.Id);
        Assert.Equal("ACME", response.CodeName);
        Assert.Equal("Acme Corp", response.Name);

        var stored = await tenantRepo.GetByIdAsync(response.Id);
        Assert.NotNull(stored);
    }

    [Fact]
    public async Task GetByIdAsync_ReturnsNullWhenNotFound()
    {
        var (service, _, _) = CreateService();

        var result = await service.GetByIdAsync(Guid.NewGuid());

        Assert.Null(result);
    }

    [Fact]
    public async Task GetByCodeNameAsync_ReturnsTenantByCodeName()
    {
        var (service, _, _) = CreateService();
        await service.CreateAsync(MakeCreateRequest("acme"));

        var result = await service.GetByCodeNameAsync("ACME");

        Assert.NotNull(result);
        Assert.Equal("ACME", result!.CodeName);
    }

    [Fact]
    public async Task AssignRoleAsync_AssignsRoleToTenant()
    {
        var (service, tenantRepo, _) = CreateService();
        var tenant = await service.CreateAsync(MakeCreateRequest());
        var roleId = Guid.NewGuid();

        await service.AssignRoleAsync(tenant.Id, roleId);

        var stored = await tenantRepo.GetByIdAsync(tenant.Id);
        Assert.NotNull(stored);
        Assert.Contains(stored!.TenantRoles, tr => tr.RoleId == roleId);
    }

    [Fact]
    public async Task GrantPermissionAsync_GrantsPermissionToTenant()
    {
        var (service, tenantRepo, _) = CreateService();
        var tenant = await service.CreateAsync(MakeCreateRequest());
        var permissionId = Guid.NewGuid();

        await service.GrantPermissionAsync(tenant.Id, permissionId);

        var stored = await tenantRepo.GetByIdAsync(tenant.Id);
        Assert.NotNull(stored);
        Assert.Contains(stored!.TenantPermissions, tp => tp.PermissionId == permissionId);
    }

    [Fact]
    public async Task AddMemberAsync_SetsUserTenantId()
    {
        var (service, tenantRepo, userRepo) = CreateService();
        var tenant = await service.CreateAsync(MakeCreateRequest());
        var user = User.Create("Alice", "M", "Smith", "alice", "alice@example.com", _hasher.Hash("pass"));
        await userRepo.AddAsync(user);

        await service.AddMemberAsync(tenant.Id, user.Id);

        var stored = await userRepo.GetByIdAsync(user.Id);
        Assert.NotNull(stored);
        Assert.Equal(tenant.Id, stored!.TenantId);
    }

    [Fact]
    public async Task RemoveMemberAsync_ClearsUserTenantId()
    {
        var (service, tenantRepo, userRepo) = CreateService();
        var tenant = await service.CreateAsync(MakeCreateRequest());
        var user = User.Create("Bob", "M", "Jones", "bob", "bob@example.com", _hasher.Hash("pass"));
        user.JoinTenant(tenant.Id);
        await userRepo.AddAsync(user);

        await service.RemoveMemberAsync(tenant.Id, user.Id);

        var stored = await userRepo.GetByIdAsync(user.Id);
        Assert.NotNull(stored);
        Assert.Null(stored!.TenantId);
    }

    [Fact]
    public async Task AddMemberAsync_WhenUserAlreadyInDifferentTenant_Throws()
    {
        var (service, _, userRepo) = CreateService();
        var tenant1 = await service.CreateAsync(MakeCreateRequest("tenant1"));
        var tenant2 = await service.CreateAsync(MakeCreateRequest("tenant2"));

        var user = User.Create("Carol", "M", "White", "carol", "carol@example.com", _hasher.Hash("pass"));
        user.JoinTenant(tenant1.Id);
        await userRepo.AddAsync(user);

        await Assert.ThrowsAsync<InvalidOperationException>(() =>
            service.AddMemberAsync(tenant2.Id, user.Id));
    }

    [Fact]
    public async Task AddMemberAsync_WhenTenantInactive_Throws()
    {
        var (service, tenantRepo, userRepo) = CreateService();
        var tenantResponse = await service.CreateAsync(MakeCreateRequest());
        var tenant = await tenantRepo.GetByIdAsync(tenantResponse.Id);
        tenant!.SetActive(false);
        await tenantRepo.UpdateAsync(tenant);

        var user = User.Create("Dan", "M", "Brown", "dan", "dan@example.com", _hasher.Hash("pass"));
        await userRepo.AddAsync(user);

        await Assert.ThrowsAsync<InvalidOperationException>(() =>
            service.AddMemberAsync(tenantResponse.Id, user.Id));
    }

    [Fact]
    public async Task RemoveMemberAsync_WhenUserNotInTenant_Throws()
    {
        var (service, _, userRepo) = CreateService();
        var tenant = await service.CreateAsync(MakeCreateRequest());
        var otherTenantId = Guid.NewGuid();

        var user = User.Create("Eve", "M", "Davis", "eve", "eve@example.com", _hasher.Hash("pass"));
        user.JoinTenant(otherTenantId);
        await userRepo.AddAsync(user);

        await Assert.ThrowsAsync<InvalidOperationException>(() =>
            service.RemoveMemberAsync(tenant.Id, user.Id));
    }

    [Fact]
    public async Task GetAllAsync_ReturnsAllTenants()
    {
        var (service, _, _) = CreateService();
        await service.CreateAsync(MakeCreateRequest("alpha"));
        await service.CreateAsync(MakeCreateRequest("beta"));

        var result = await service.GetAllAsync();

        Assert.Equal(2, result.Count);
    }
}
