using Microsoft.EntityFrameworkCore;
using Knights.Application.Common.Interfaces;
using Knights.Domain.Identity;
using Knights.Domain.Tenants;
using Knights.Domain.Dashboards;
using Knights.Infrastructure.Persistence.Configurations;
using Microsoft.Extensions.Options;

namespace Knights.Infrastructure.Persistence;

public sealed class KnightsDbContext(
    DbContextOptions<KnightsDbContext> options,
    ITenantContext tenantContext,
    IOptions<PersistenceDateTimeOptions> dateTimeOptions) : DbContext(options)
{
    private readonly ITenantContext _tenantContext = tenantContext;
    private readonly PersistenceDateTimeOptions _dateTimeOptions = dateTimeOptions.Value;

    public DbSet<User> Users => Set<User>();
    public DbSet<Role> Roles => Set<Role>();
    public DbSet<Permission> Permissions => Set<Permission>();
    public DbSet<UserRole> UserRoles => Set<UserRole>();
    public DbSet<UserPermission> UserPermissions => Set<UserPermission>();
    public DbSet<Tenant> Tenants => Set<Tenant>();
    public DbSet<TenantRole> TenantRoles => Set<TenantRole>();
    public DbSet<TenantPermission> TenantPermissions => Set<TenantPermission>();
    public DbSet<FeatureCatalogItem> FeatureCatalogItems => Set<FeatureCatalogItem>();
    public DbSet<TenantFeatureSelection> TenantFeatureSelections => Set<TenantFeatureSelection>();
    public DbSet<RolePermission> RolePermissions => Set<RolePermission>();
    public DbSet<Dashboard> Dashboards => Set<Dashboard>();
    public DbSet<DashboardWidget> DashboardWidgets => Set<DashboardWidget>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<User>().HasQueryFilter(
            u => !u.IsDeleted && (_tenantContext.TenantId == null || u.TenantId == _tenantContext.TenantId));
        modelBuilder.Entity<Dashboard>().HasQueryFilter(
            dashboard => !dashboard.IsDeleted && (_tenantContext.TenantId == null || dashboard.TenantId == _tenantContext.TenantId));

        modelBuilder.ApplyConfigurationsFromAssembly(typeof(KnightsDbContext).Assembly);
        modelBuilder.ApplyDateTimeUtcOffsetConversions(_dateTimeOptions.LoadOffset);

        base.OnModelCreating(modelBuilder);
    }
}
