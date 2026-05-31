using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Knights.Domain.Identity;

namespace Knights.Infrastructure.Persistence.Configurations;

internal sealed class PermissionConfiguration : IEntityTypeConfiguration<Permission>
{
    public void Configure(EntityTypeBuilder<Permission> builder)
    {
        builder.ToTable("Permissions");
        builder.ConfigureBaseEntity();

        builder.Property(permission => permission.CodeName).IsRequired().HasMaxLength(256);
        builder.Property(permission => permission.DisplayName).IsRequired().HasMaxLength(256);
        builder.Property(permission => permission.Description).IsRequired().HasMaxLength(1024);

        builder.HasIndex(permission => permission.CodeName).IsUnique();

        builder.Navigation(permission => permission.Users)
            .HasField("_users")
            .UsePropertyAccessMode(PropertyAccessMode.Field);
    }
}
