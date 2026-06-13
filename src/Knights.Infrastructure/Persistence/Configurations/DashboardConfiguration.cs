using Knights.Domain.Dashboards;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Knights.Infrastructure.Persistence.Configurations;

internal sealed class DashboardConfiguration : IEntityTypeConfiguration<Dashboard>
{
    public void Configure(EntityTypeBuilder<Dashboard> builder)
    {
        builder.ToTable("Dashboards");
        builder.ConfigureBaseEntity();
        builder.Property(dashboard => dashboard.Name).IsRequired().HasMaxLength(160);
        builder.Property(dashboard => dashboard.Slug).IsRequired().HasMaxLength(180);
        builder.HasIndex(dashboard => new { dashboard.OwnerUserId, dashboard.Slug }).IsUnique();
        builder.HasMany(dashboard => dashboard.Widgets).WithOne().HasForeignKey(widget => widget.DashboardId).OnDelete(DeleteBehavior.Cascade);
        builder.Navigation(dashboard => dashboard.Widgets).HasField("_widgets").UsePropertyAccessMode(PropertyAccessMode.Field);
    }
}
