using Microsoft.EntityFrameworkCore.Storage.ValueConversion;

namespace Knights.Infrastructure.Persistence;

internal sealed class DateTimeUtcOffsetConverter(TimeSpan loadOffset)
    : ValueConverter<DateTime, DateTime>(
        value => ToUtc(value, loadOffset),
        value => FromUtc(value, loadOffset))
{
    public static DateTime ToUtc(DateTime value, TimeSpan loadOffset)
    {
        if (value == DateTime.MinValue || value == DateTime.MaxValue)
        {
            return DateTime.SpecifyKind(value, DateTimeKind.Utc);
        }

        return value.Kind switch
        {
            DateTimeKind.Utc => value,
            DateTimeKind.Local => value.ToUniversalTime(),
            _ => DateTime.SpecifyKind(value - loadOffset, DateTimeKind.Utc)
        };
    }

    public static DateTime FromUtc(DateTime value, TimeSpan loadOffset)
    {
        if (value == DateTime.MinValue || value == DateTime.MaxValue)
        {
            return DateTime.SpecifyKind(value, DateTimeKind.Unspecified);
        }

        var utcValue = value.Kind == DateTimeKind.Utc
            ? value
            : DateTime.SpecifyKind(value, DateTimeKind.Utc);

        return DateTime.SpecifyKind(utcValue + loadOffset, DateTimeKind.Unspecified);
    }
}

internal sealed class NullableDateTimeUtcOffsetConverter(TimeSpan loadOffset)
    : ValueConverter<DateTime?, DateTime?>(
        value => value.HasValue ? DateTimeUtcOffsetConverter.ToUtc(value.Value, loadOffset) : null,
        value => value.HasValue ? DateTimeUtcOffsetConverter.FromUtc(value.Value, loadOffset) : null);
