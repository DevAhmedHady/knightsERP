namespace Knights.Infrastructure.Tests;

using Knights.Application.Common.Interfaces;
using Knights.Domain.Identity;
using Knights.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;

public sealed class TenantIsolationTests
{
    [Fact]
    public void GetAll_TenantScoped_ReturnsOnlyOwnTenantUsers()
    {
        var databaseName = Guid.NewGuid().ToString();
        var tenant1Id = Guid.NewGuid();
        var tenant2Id = Guid.NewGuid();

        using (var seedContext = CreateContext(databaseName))
        {
            seedContext.Users.Add(CreateUser("Tenant", "One", "User", "tenant1user", "tenant1@example.com", tenant1Id));
            seedContext.Users.Add(CreateUser("Tenant", "Two", "User", "tenant2user", "tenant2@example.com", tenant2Id));
            seedContext.SaveChanges();
        }

        using var context = CreateContext(databaseName, tenant1Id);

        var users = context.Users.ToList();

        var user = Assert.Single(users);
        Assert.Equal(tenant1Id, user.TenantId);
    }

    [Fact]
    public void GetAll_SystemAdmin_ReturnsAllUsers()
    {
        var databaseName = Guid.NewGuid().ToString();
        var tenant1Id = Guid.NewGuid();
        var tenant2Id = Guid.NewGuid();

        using (var seedContext = CreateContext(databaseName))
        {
            seedContext.Users.Add(CreateUser("Tenant", "One", "User", "tenant1user", "tenant1@example.com", tenant1Id));
            seedContext.Users.Add(CreateUser("Tenant", "Two", "User", "tenant2user", "tenant2@example.com", tenant2Id));
            seedContext.Users.Add(CreateUser("System", "No", "Tenant", "systemuser", "system@example.com"));
            seedContext.SaveChanges();
        }

        using var context = CreateContext(databaseName);

        var users = context.Users.ToList();

        Assert.Equal(3, users.Count);
        Assert.Contains(users, user => user.TenantId == tenant1Id);
        Assert.Contains(users, user => user.TenantId == tenant2Id);
        Assert.Contains(users, user => user.TenantId is null);
    }

    [Fact]
    public void GetById_TenantScoped_CannotReadOtherTenantUser()
    {
        var databaseName = Guid.NewGuid().ToString();
        var tenant1Id = Guid.NewGuid();
        var tenant2Id = Guid.NewGuid();
        Guid userId;

        using (var seedContext = CreateContext(databaseName))
        {
            var user = CreateUser("Tenant", "Two", "User", "tenant2user", "tenant2@example.com", tenant2Id);
            seedContext.Users.Add(user);
            seedContext.SaveChanges();
            userId = user.Id;
        }

        using var context = CreateContext(databaseName, tenant1Id);

        var result = context.Users.Find(userId);

        Assert.Null(result);
    }

    [Fact]
    public void GetById_TenantScoped_CanReadOwnTenantUser()
    {
        var databaseName = Guid.NewGuid().ToString();
        var tenant1Id = Guid.NewGuid();
        Guid userId;

        using (var seedContext = CreateContext(databaseName))
        {
            var user = CreateUser("Tenant", "One", "User", "tenant1user", "tenant1@example.com", tenant1Id);
            seedContext.Users.Add(user);
            seedContext.SaveChanges();
            userId = user.Id;
        }

        using var context = CreateContext(databaseName, tenant1Id);

        var result = context.Users.Find(userId);

        Assert.NotNull(result);
        Assert.Equal(tenant1Id, result.TenantId);
    }

    private static KnightsDbContext CreateContext(string databaseName, Guid? tenantId = null)
    {
        var fakeTenant = new FakeTenantContext { TenantId = tenantId };
        var options = new DbContextOptionsBuilder<KnightsDbContext>()
            .UseInMemoryDatabase(databaseName)
            .Options;

        return new KnightsDbContext(options, fakeTenant, Options.Create(new PersistenceDateTimeOptions()));
    }

    private static User CreateUser(
        string firstName,
        string midName,
        string lastName,
        string userName,
        string email,
        Guid? tenantId = null)
    {
        var user = User.Create(firstName, midName, lastName, userName, email, "password-hash");

        if (tenantId.HasValue)
        {
            user.JoinTenant(tenantId.Value);
        }

        return user;
    }

    private sealed class FakeTenantContext : ITenantContext
    {
        public Guid? TenantId { get; set; }
    }
}
