using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Knights.Domain.Identity;

namespace Knights.Infrastructure.Persistence.Configurations;

internal sealed class RoleConfiguration : IEntityTypeConfiguration<Role>
{
    public void Configure(EntityTypeBuilder<Role> builder)
    {
        builder.ToTable("Roles");
        builder.ConfigureBaseEntity();

        builder.Property(role => role.CodeName).IsRequired().HasMaxLength(256);
        builder.Property(role => role.Name).IsRequired().HasMaxLength(256);
        builder.Property(role => role.Description).IsRequired().HasMaxLength(1024);

        builder.HasIndex(role => role.CodeName).IsUnique();

        builder.Navigation(role => role.Users)
            .HasField("_users")
            .UsePropertyAccessMode(PropertyAccessMode.Field);

        builder.Navigation(role => role.Permissions)
            .HasField("_permissions")
            .UsePropertyAccessMode(PropertyAccessMode.Field);
    }
}
