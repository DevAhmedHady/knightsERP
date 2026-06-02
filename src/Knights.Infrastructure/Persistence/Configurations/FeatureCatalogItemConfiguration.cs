using Knights.Domain.Tenants;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Knights.Infrastructure.Persistence.Configurations;

internal sealed class FeatureCatalogItemConfiguration : IEntityTypeConfiguration<FeatureCatalogItem>
{
    public void Configure(EntityTypeBuilder<FeatureCatalogItem> builder)
    {
        builder.ToTable("FeatureCatalogItems");
        builder.ConfigureBaseEntity();

        builder.Property(item => item.Key).IsRequired().HasMaxLength(128);
        builder.Property(item => item.Name).IsRequired().HasMaxLength(256);
        builder.Property(item => item.Description).IsRequired().HasMaxLength(1024);
        builder.Property(item => item.Category).IsRequired().HasMaxLength(128);
        builder.Property(item => item.IconKey).IsRequired().HasMaxLength(128).HasDefaultValue(string.Empty);
        builder.Property(item => item.TagsSerialized).IsRequired().HasMaxLength(1024).HasDefaultValue(string.Empty);
        builder.Property(item => item.DependencyKeysSerialized).IsRequired().HasMaxLength(1024).HasDefaultValue(string.Empty);
        builder.Property(item => item.SettingsSchemaJson).IsRequired().HasColumnType("jsonb").HasDefaultValueSql("'{}'::jsonb");
        builder.Property(item => item.DefaultSettingsJson).IsRequired().HasColumnType("jsonb").HasDefaultValueSql("'{}'::jsonb");

        builder.HasIndex(item => item.Key).IsUnique();
        builder.HasIndex(item => new { item.IsPublished, item.IsRetired, item.DisplayOrder });
        builder.HasIndex(item => new { item.Category, item.IsCore });
    }
}
