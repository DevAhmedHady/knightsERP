using Knights.Domain.Dashboards;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Knights.Infrastructure.Persistence.Configurations;

internal sealed class DashboardWidgetConfiguration : IEntityTypeConfiguration<DashboardWidget>
{
    public void Configure(EntityTypeBuilder<DashboardWidget> builder)
    {
        builder.ToTable("DashboardWidgets");
        builder.ConfigureBaseEntity();
        builder.Property(widget => widget.Title).IsRequired().HasMaxLength(160);
        builder.Property(widget => widget.DataSourceKey).IsRequired().HasMaxLength(80);
        builder.Property(widget => widget.QuerySpecJson).IsRequired().HasColumnType("jsonb");
        builder.Property(widget => widget.VisualizationConfigJson).IsRequired().HasColumnType("jsonb");
    }
}
