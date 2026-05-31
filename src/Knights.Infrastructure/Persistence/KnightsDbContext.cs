using Microsoft.EntityFrameworkCore;
using Knights.Domain.Identity;
using Knights.Domain.Tenants;

namespace Knights.Infrastructure.Persistence;

public sealed class KnightsDbContext(DbContextOptions<KnightsDbContext> options) : DbContext(options)
{
    public DbSet<User> Users => Set<User>();
    public DbSet<Role> Roles => Set<Role>();
    public DbSet<Permission> Permissions => Set<Permission>();
    public DbSet<UserRole> UserRoles => Set<UserRole>();
    public DbSet<UserPermission> UserPermissions => Set<UserPermission>();
    public DbSet<Tenant> Tenants => Set<Tenant>();
    public DbSet<TenantRole> TenantRoles => Set<TenantRole>();
    public DbSet<RolePermission> RolePermissions => Set<RolePermission>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.ApplyConfigurationsFromAssembly(typeof(KnightsDbContext).Assembly);
        base.OnModelCreating(modelBuilder);
    }
}
