using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Knights.Domain.Identity;

namespace Knights.Infrastructure.Persistence.Configurations;

internal sealed class UserPermissionConfiguration : IEntityTypeConfiguration<UserPermission>
{
    public void Configure(EntityTypeBuilder<UserPermission> builder)
    {
        builder.ToTable("UserPermissions");
        builder.ConfigureBaseEntity();

        builder.HasOne(userPermission => userPermission.User)
            .WithMany(user => user.UserPermissions)
            .HasForeignKey(userPermission => userPermission.UserId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasOne(userPermission => userPermission.Permission)
            .WithMany(permission => permission.Users)
            .HasForeignKey(userPermission => userPermission.PermissionId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasIndex(userPermission => new { userPermission.UserId, userPermission.PermissionId }).IsUnique();
    }
}
