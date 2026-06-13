using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Knights.Domain.Common;
using Microsoft.EntityFrameworkCore;

namespace Knights.Infrastructure.Persistence.Configurations;

internal static class EntityConfigurationExtensions
{
    public static void ConfigureBaseEntity<TEntity>(this EntityTypeBuilder<TEntity> builder)
        where TEntity : BaseEntity
    {
        builder.HasKey(entity => entity.Id);
        builder.Property(entity => entity.Id).ValueGeneratedNever();

        builder.Property<string?>("_additionalPropertiesJson")
            .HasColumnName("AdditionalProperties");
    }
}
