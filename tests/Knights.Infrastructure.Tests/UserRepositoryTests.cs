namespace Knights.Infrastructure.Tests;

using Knights.Application.Common.Interfaces;
using Knights.Domain.Identity;
using Knights.Infrastructure.Persistence;
using Knights.Infrastructure.Persistence.Repositories;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;

public sealed class UserRepositoryTests
{
    [Fact]
    public async Task DeleteAsync_ExcludesUserFromGetAllAndGetById()
    {
        await using var context = CreateContext();
        var repository = new UserRepository(context);
        var user = User.Create("Ada", "Byron", "Lovelace", "ada.lovelace", "ada@example.com", "password-hash");
        await repository.AddAsync(user);

        await repository.DeleteAsync(user.Id);

        var users = await repository.GetAllAsync();
        var deletedUser = await repository.GetByIdAsync(user.Id);

        Assert.Empty(users);
        Assert.Null(deletedUser);
    }

    private static KnightsDbContext CreateContext()
    {
        var options = new DbContextOptionsBuilder<KnightsDbContext>()
            .UseInMemoryDatabase(Guid.NewGuid().ToString())
            .Options;

        return new KnightsDbContext(
            options,
            new FakeTenantContext(),
            Options.Create(new PersistenceDateTimeOptions()));
    }

    private sealed class FakeTenantContext : ITenantContext
    {
        public Guid? TenantId => null;
    }
}
