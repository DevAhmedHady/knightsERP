using Knights.Application.Tenants;
using Knights.Application.Tenants.Requests;
using Knights.Application.Tests.Fakes;
using Knights.Domain.Identity;

namespace Knights.Application.Tests;

public class TenantServiceTests
{
    private readonly FakePasswordHasher _hasher = new();
    private readonly FakeTenantContext _tenantContext = new();

    private (TenantService Service, InMemoryTenantRepository TenantRepo, InMemoryUserRepository UserRepo) CreateService()
    {
        var tenantRepo = new InMemoryTenantRepository();
        var userRepo = new InMemoryUserRepository();
        var service = new TenantService(tenantRepo, userRepo, _tenantContext);
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
    public async Task DeactivateAsync_MarksTenantInactive()
    {
        var (service, tenantRepo, _) = CreateService();
        var tenant = await service.CreateAsync(MakeCreateRequest());

        await service.DeactivateAsync(tenant.Id);

        var stored = await tenantRepo.GetByIdAsync(tenant.Id);
        Assert.NotNull(stored);
        Assert.False(stored!.IsActive);
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

    [Fact]
    public async Task GetCurrentSetupAsync_ReturnsProgressAndUnlocksAtFiftyPercent()
    {
        var (service, _, _) = CreateService();
        var tenant = await service.CreateAsync(MakeCreateRequest("omega"));
        _tenantContext.TenantId = tenant.Id;

        await service.ConfigureCurrentEnvironmentAsync(new ConfigureTenantEnvironmentRequest("Omega HQ", "nightwatch", "Operations world"));
        await service.AssignRoleAsync(tenant.Id, Guid.NewGuid());

        var summary = await service.GetCurrentSetupAsync();

        Assert.Equal(75, summary.ProgressPercent);
        Assert.True(summary.IsUnlocked);
        Assert.False(summary.IsComplete);
    }

    [Fact]
    public async Task SelectCurrentFeatureAsync_RequiresDependencies()
    {
        var (service, _, _) = CreateService();
        var tenant = await service.CreateAsync(MakeCreateRequest("gamma"));

        var baseFeature = await service.CreateCatalogFeatureAsync(new CreateFeatureCatalogItemRequest(
            "BASE_WORLD",
            "Base World",
            "Base module",
            "Foundation",
            "pi pi-globe",
            ["core"],
            [],
            "{}",
            """{"brandingMode":"basic"}""",
            30,
            true,
            1,
            true));

        _tenantContext.TenantId = null;
        var dependent = await service.CreateCatalogFeatureAsync(new CreateFeatureCatalogItemRequest(
            "EVENTS",
            "Events",
            "Event engine",
            "Gameplay",
            "pi pi-bolt",
            ["events"],
            ["BASE_WORLD"],
            "{}",
            """{"retentionDays":30}""",
            15,
            false,
            2,
            true));

        _tenantContext.TenantId = tenant.Id;

        await Assert.ThrowsAsync<InvalidOperationException>(() => service.SelectCurrentFeatureAsync(dependent.Id));

        await service.SelectCurrentFeatureAsync(baseFeature.Id);
        var summary = await service.SelectCurrentFeatureAsync(dependent.Id);

        Assert.Equal(2, summary.SelectedFeatures.Count);
    }

    [Fact]
    public async Task CreateCatalogFeatureAsync_RequiresSystemAdmin()
    {
        var (service, _, _) = CreateService();
        var tenant = await service.CreateAsync(MakeCreateRequest("delta"));
        _tenantContext.TenantId = tenant.Id;

        await Assert.ThrowsAsync<InvalidOperationException>(() => service.CreateCatalogFeatureAsync(new CreateFeatureCatalogItemRequest(
            "OPS",
            "Ops",
            "Ops module",
            "Foundation",
            "pi pi-map",
            ["ops"],
            [],
            "{}",
            "{}",
            10,
            false,
            1,
            true)));
    }

    [Fact]
    public async Task CreateCatalogFeatureAsync_PersistsAdvancedMetadata()
    {
        var (service, _, _) = CreateService();

        var feature = await service.CreateCatalogFeatureAsync(new CreateFeatureCatalogItemRequest(
            "ANALYTICS",
            "Analytics",
            "Insight tools",
            "Insights",
            "pi pi-chart-bar",
            ["analytics", "health"],
            [],
            """{"type":"object"}""",
            """{"refreshMinutes":15}""",
            25,
            false,
            3,
            true));

        Assert.Equal("pi pi-chart-bar", feature.IconKey);
        Assert.Equal(2, feature.Tags.Count);
        Assert.Equal(25, feature.SetupWeight);
        Assert.Contains("refreshMinutes", feature.DefaultSettingsJson);
    }

    [Fact]
    public async Task UpdateCurrentFeatureSettingsAsync_PersistsTenantSpecificSettings()
    {
        var (service, _, _) = CreateService();
        var tenant = await service.CreateAsync(MakeCreateRequest("sigma"));

        var feature = await service.CreateCatalogFeatureAsync(new CreateFeatureCatalogItemRequest(
            "FIELD",
            "Field",
            "Field ops",
            "Operations",
            "pi pi-map",
            ["field"],
            [],
            """{"type":"object","properties":{"zones":{"type":"string"},"active":{"type":"boolean"}}}""",
            """{"zones":"","active":false}""",
            10,
            false,
            2,
            true));

        _tenantContext.TenantId = tenant.Id;
        await service.SelectCurrentFeatureAsync(feature.Id);

        var summary = await service.UpdateCurrentFeatureSettingsAsync(feature.Id, new UpdateTenantFeatureSettingsRequest("""{"zones":"north","active":true}"""));

        Assert.Contains(summary.SelectedFeatures, item => item.Id == feature.Id && item.SettingsJson.Contains("north"));
    }
}
