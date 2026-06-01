using Knights.Application.Common.Interfaces;
using Knights.Domain.Identity;
using Microsoft.EntityFrameworkCore;

namespace Knights.Infrastructure.Persistence.Seeding;

public static class DataSeeder
{
    public static async Task SeedAsync(KnightsDbContext dbContext, IPasswordHasher passwordHasher)
    {
        await SeedRolesAsync(dbContext);
        await SeedAdminUserAsync(dbContext, passwordHasher);
    }

    private static async Task SeedRolesAsync(KnightsDbContext dbContext)
    {
        if (await dbContext.Roles.AnyAsync())
            return;

        var adminRole = Role.Create(
            name: "Admin",
            description: "Administrator with full access",
            isStatic: true,
            isDefault: false,
            isActive: true);

        var userRole = Role.Create(
            name: "User",
            description: "Regular user",
            isStatic: true,
            isDefault: true,
            isActive: true);

        dbContext.Roles.AddRange(adminRole, userRole);
        await dbContext.SaveChangesAsync();
    }

    private static async Task SeedAdminUserAsync(KnightsDbContext dbContext, IPasswordHasher passwordHasher)
    {
        if (await dbContext.Users.AnyAsync())
            return;

        const string password = "Admin@123456";
        var passwordHash = passwordHasher.Hash(password);

        var adminUser = User.Create(
            firstName: "Admin",
            midName: "System",
            lastName: "User",
            userName: "admin",
            email: "admin@knights.local",
            passwordHash: passwordHash,
            isEmailConfirmed: true);

        dbContext.Users.Add(adminUser);
        await dbContext.SaveChangesAsync();

        var adminRole = await dbContext.Roles.FirstAsync(r => r.CodeName == "ADMIN");
        adminUser.AssignRole(adminRole.Id);

        await dbContext.SaveChangesAsync();
    }
}
