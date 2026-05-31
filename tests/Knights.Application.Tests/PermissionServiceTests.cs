using Knights.Application.Common.Interfaces;
using Knights.Application.Permissions;
using Knights.Application.Permissions.Requests;
using Knights.Domain.Identity;

namespace Knights.Application.Tests;

public class PermissionServiceTests
{
    [Fact]
    public async Task CreateAsync_CreatesPermissionAndMapsResponse()
    {
        var repository = new InMemoryPermissionRepository();
        var service = new PermissionService(repository);

        var response = await service.CreateAsync(new CreatePermissionRequest("read users", "Read Users", "Can read user data"));

        Assert.NotEqual(Guid.Empty, response.Id);
        Assert.Equal("READ_USERS", response.CodeName);
        Assert.Equal("Read Users", response.DisplayName);
        Assert.Equal("Can read user data", response.Description);
    }

    [Fact]
    public async Task UpdateAsync_UpdatesAndReturnsResponse()
    {
        var repository = new InMemoryPermissionRepository();
        var service = new PermissionService(repository);
        var permission = Permission.Create("write users", "Write Users", "Can write user data");
        await repository.AddAsync(permission);

        var response = await service.UpdateAsync(permission.Id, new UpdatePermissionRequest("Manage Users", "Can manage user data"));

        Assert.Equal("Manage Users", response.DisplayName);
        Assert.Equal("Can manage user data", response.Description);
    }

    [Fact]
    public async Task UpdateAsync_ThrowsIfNotFound()
    {
        var repository = new InMemoryPermissionRepository();
        var service = new PermissionService(repository);

        await Assert.ThrowsAsync<InvalidOperationException>(
            () => service.UpdateAsync(Guid.NewGuid(), new UpdatePermissionRequest("X", "Y")));
    }

    [Fact]
    public async Task GetAllAsync_ReturnsAllPermissions()
    {
        var repository = new InMemoryPermissionRepository();
        var service = new PermissionService(repository);
        await repository.AddAsync(Permission.Create("read", "Read", "Read access"));
        await repository.AddAsync(Permission.Create("write", "Write", "Write access"));

        var permissions = await service.GetAllAsync();

        Assert.Equal(2, permissions.Count);
    }

    private sealed class InMemoryPermissionRepository : IPermissionRepository
    {
        private readonly Dictionary<Guid, Permission> _permissions = [];

        public Task<Permission?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default)
        {
            _permissions.TryGetValue(id, out var permission);
            return Task.FromResult(permission);
        }

        public Task<IReadOnlyCollection<Permission>> GetAllAsync(CancellationToken cancellationToken = default)
        {
            IReadOnlyCollection<Permission> result = _permissions.Values.ToArray();
            return Task.FromResult(result);
        }

        public Task<Permission?> GetByCodeNameAsync(string codeName, CancellationToken cancellationToken = default)
        {
            var permission = _permissions.Values.FirstOrDefault(p => p.CodeName == codeName);
            return Task.FromResult(permission);
        }

        public Task AddAsync(Permission permission, CancellationToken cancellationToken = default)
        {
            _permissions[permission.Id] = permission;
            return Task.CompletedTask;
        }

        public Task UpdateAsync(Permission permission, CancellationToken cancellationToken = default)
        {
            _permissions[permission.Id] = permission;
            return Task.CompletedTask;
        }

        public Task DeleteAsync(Guid id, CancellationToken cancellationToken = default)
        {
            _permissions.Remove(id);
            return Task.CompletedTask;
        }
    }
}
