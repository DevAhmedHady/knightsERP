using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Knights.Domain.Identity;

namespace Knights.Infrastructure.Persistence.Configurations;

internal sealed class UserConfiguration : IEntityTypeConfiguration<User>
{
    public void Configure(EntityTypeBuilder<User> builder)
    {
        builder.ToTable("Users");
        builder.ConfigureBaseEntity();

        builder.Property(user => user.UserName).IsRequired().HasMaxLength(256);
        builder.Property(user => user.Email).IsRequired().HasMaxLength(256);
        builder.Property(user => user.PasswordHash).HasMaxLength(512);

        builder.HasIndex(user => user.UserName).IsUnique();
        builder.HasIndex(user => user.Email).IsUnique();

        builder.OwnsOne(user => user.Name, name =>
        {
            name.Property(value => value.FirstName).HasColumnName("FirstName").IsRequired().HasMaxLength(128);
            name.Property(value => value.MidName).HasColumnName("MidName").IsRequired().HasMaxLength(128);
            name.Property(value => value.LastName).HasColumnName("LastName").IsRequired().HasMaxLength(128);
        });
        builder.Navigation(user => user.Name).IsRequired();

        builder.Navigation(user => user.UserRoles)
            .HasField("_roles")
            .UsePropertyAccessMode(PropertyAccessMode.Field);

        builder.Navigation(user => user.UserPermissions)
            .HasField("_permissions")
            .UsePropertyAccessMode(PropertyAccessMode.Field);
    }
}
