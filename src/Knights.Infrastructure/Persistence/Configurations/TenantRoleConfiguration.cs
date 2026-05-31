using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Knights.Domain.Tenants;

namespace Knights.Infrastructure.Persistence.Configurations;

internal sealed class TenantRoleConfiguration : IEntityTypeConfiguration<TenantRole>
{
    public void Configure(EntityTypeBuilder<TenantRole> builder)
    {
        builder.ToTable("TenantRoles");
        builder.ConfigureBaseEntity();

        builder.HasOne(tenantRole => tenantRole.Tenant)
            .WithMany(tenant => tenant.TenantRoles)
            .HasForeignKey(tenantRole => tenantRole.TenantId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasOne(tenantRole => tenantRole.Role)
            .WithMany()
            .HasForeignKey(tenantRole => tenantRole.RoleId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasIndex(tenantRole => new { tenantRole.TenantId, tenantRole.RoleId }).IsUnique();
    }
}
