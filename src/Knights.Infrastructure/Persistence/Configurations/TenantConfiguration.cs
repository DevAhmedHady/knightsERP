using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Knights.Domain.Tenants;

namespace Knights.Infrastructure.Persistence.Configurations;

internal sealed class TenantConfiguration : IEntityTypeConfiguration<Tenant>
{
    public void Configure(EntityTypeBuilder<Tenant> builder)
    {
        builder.ToTable("Tenants");
        builder.ConfigureBaseEntity();

        builder.Property(tenant => tenant.CodeName).IsRequired().HasMaxLength(256);
        builder.Property(tenant => tenant.Name).IsRequired().HasMaxLength(256);
        builder.Property(tenant => tenant.Description).IsRequired().HasMaxLength(1024);

        builder.HasIndex(tenant => tenant.CodeName).IsUnique();

        builder.Navigation(tenant => tenant.TenantRoles)
            .HasField("_roles")
            .UsePropertyAccessMode(PropertyAccessMode.Field);

        builder.Navigation(tenant => tenant.TenantPermissions)
            .HasField("_permissions")
            .UsePropertyAccessMode(PropertyAccessMode.Field);
    }
}
