using Knights.Domain.Tenants;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Knights.Infrastructure.Persistence.Configurations;

internal sealed class TenantFeatureSelectionConfiguration : IEntityTypeConfiguration<TenantFeatureSelection>
{
    public void Configure(EntityTypeBuilder<TenantFeatureSelection> builder)
    {
        builder.ToTable("TenantFeatureSelections");
        builder.ConfigureBaseEntity();
        builder.Property(selection => selection.SettingsJson).IsRequired().HasColumnType("jsonb").HasDefaultValueSql("'{}'::jsonb");

        builder.HasOne(selection => selection.Tenant)
            .WithMany(tenant => tenant.TenantFeatureSelections)
            .HasForeignKey(selection => selection.TenantId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasOne(selection => selection.FeatureCatalogItem)
            .WithMany()
            .HasForeignKey(selection => selection.FeatureCatalogItemId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasIndex(selection => new { selection.TenantId, selection.FeatureCatalogItemId }).IsUnique();
    }
}
