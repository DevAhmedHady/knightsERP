using Microsoft.EntityFrameworkCore;

namespace Knights.Infrastructure.Persistence.Configurations;

internal static class DateTimeConfigurationExtensions
{
    public static void ApplyDateTimeUtcOffsetConversions(this ModelBuilder modelBuilder, TimeSpan loadOffset)
    {
        var converter = new DateTimeUtcOffsetConverter(loadOffset);
        var nullableConverter = new NullableDateTimeUtcOffsetConverter(loadOffset);

        foreach (var property in modelBuilder.Model.GetEntityTypes().SelectMany(entityType => entityType.GetProperties()))
        {
            if (property.ClrType == typeof(DateTime))
            {
                property.SetValueConverter(converter);
            }
            else if (property.ClrType == typeof(DateTime?))
            {
                property.SetValueConverter(nullableConverter);
            }
        }
    }
}
