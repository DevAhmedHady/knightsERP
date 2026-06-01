using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Knights.Domain.Tenants;

namespace Knights.Infrastructure.Persistence.Configurations;

internal sealed class TenantPermissionConfiguration : IEntityTypeConfiguration<TenantPermission>
{
    public void Configure(EntityTypeBuilder<TenantPermission> builder)
    {
        builder.ToTable("TenantPermissions");
        builder.ConfigureBaseEntity();

        builder.HasOne(tp => tp.Tenant)
            .WithMany(tenant => tenant.TenantPermissions)
            .HasForeignKey(tp => tp.TenantId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasOne(tp => tp.Permission)
            .WithMany()
            .HasForeignKey(tp => tp.PermissionId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasIndex(tp => new { tp.TenantId, tp.PermissionId }).IsUnique();
    }
}
