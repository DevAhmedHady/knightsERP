using Knights.Application.Common.Interfaces;
using Knights.Application.Roles;
using Knights.Application.Roles.Requests;
using Knights.Domain.Identity;

namespace Knights.Application.Tests;

public class RoleServiceTests
{
    [Fact]
    public async Task CreateAsync_CreatesRoleAndMapsResponse()
    {
        var repository = new InMemoryRoleRepository();
        var service = new RoleService(repository);

        var response = await service.CreateAsync(new CreateRoleRequest("Admin", "Administrator role"));

        Assert.NotEqual(Guid.Empty, response.Id);
        Assert.Equal("Admin", response.Name);
        Assert.Equal("ADMIN", response.CodeName);
        Assert.Equal("Administrator role", response.Description);
        Assert.True(response.IsActive);
    }

    [Fact]
    public async Task AssignPermissionAsync_IsIdempotent()
    {
        var repository = new InMemoryRoleRepository();
        var service = new RoleService(repository);
        var role = Role.Create("Admin", "Administrator role", isStatic: false, isDefault: false);
        await repository.AddAsync(role);
        var permissionId = Guid.NewGuid();

        await service.AssignPermissionAsync(role.Id, permissionId);
        var response = await service.AssignPermissionAsync(role.Id, permissionId);

        Assert.Single(response.PermissionIds);
        Assert.Equal(permissionId, response.PermissionIds.Single());
    }

    [Fact]
    public async Task RemovePermissionAsync_RemovesPermission()
    {
        var repository = new InMemoryRoleRepository();
        var service = new RoleService(repository);
        var role = Role.Create("Admin", "Administrator role", isStatic: false, isDefault: false);
        var permissionId = Guid.NewGuid();
        role.AssignPermission(permissionId);
        await repository.AddAsync(role);

        var response = await service.RemovePermissionAsync(role.Id, permissionId);

        Assert.Empty(response.PermissionIds);
    }

    [Fact]
    public async Task UpdateAsync_ThrowsIfNotFound()
    {
        var repository = new InMemoryRoleRepository();
        var service = new RoleService(repository);

        await Assert.ThrowsAsync<InvalidOperationException>(
            () => service.UpdateAsync(Guid.NewGuid(), new UpdateRoleRequest("Name", "Desc")));
    }

    [Fact]
    public async Task GetAllAsync_ReturnsAllRoles()
    {
        var repository = new InMemoryRoleRepository();
        var service = new RoleService(repository);
        await repository.AddAsync(Role.Create("Admin", "Admin role", isStatic: false, isDefault: false));
        await repository.AddAsync(Role.Create("Viewer", "Viewer role", isStatic: false, isDefault: false));

        var roles = await service.GetAllAsync();

        Assert.Equal(2, roles.Count);
    }

    private sealed class InMemoryRoleRepository : IRoleRepository
    {
        private readonly Dictionary<Guid, Role> _roles = [];

        public Task<Role?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default)
        {
            _roles.TryGetValue(id, out var role);
            return Task.FromResult(role);
        }

        public Task<IReadOnlyCollection<Role>> GetAllAsync(CancellationToken cancellationToken = default)
        {
            IReadOnlyCollection<Role> result = _roles.Values.ToArray();
            return Task.FromResult(result);
        }

        public Task<Role?> GetWithPermissionsAsync(Guid id, CancellationToken cancellationToken = default)
        {
            _roles.TryGetValue(id, out var role);
            return Task.FromResult(role);
        }

        public Task AddAsync(Role role, CancellationToken cancellationToken = default)
        {
            _roles[role.Id] = role;
            return Task.CompletedTask;
        }

        public Task UpdateAsync(Role role, CancellationToken cancellationToken = default)
        {
            _roles[role.Id] = role;
            return Task.CompletedTask;
        }

        public Task DeleteAsync(Guid id, CancellationToken cancellationToken = default)
        {
            _roles.Remove(id);
            return Task.CompletedTask;
        }
    }
}
