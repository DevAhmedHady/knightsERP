namespace Knights.Infrastructure.Tests;

using Knights.Application.Common.Interfaces;
using Knights.Domain.Identity;
using Knights.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Storage.ValueConversion;
using Microsoft.Extensions.Options;

public sealed class DateTimeConversionTests
{
    [Fact]
    public void DateTimeConverter_StoresUnspecifiedValuesAsUtcUsingConfiguredOffset()
    {
        using var context = CreateContext(TimeSpan.FromHours(2));
        var converter = GetConverter<DateTime?>(context, nameof(User.LastLoginDate));
        var appLocalValue = new DateTime(2026, 6, 2, 14, 30, 0, DateTimeKind.Unspecified);

        var providerValue = Assert.IsType<DateTime>(converter.ConvertToProvider(appLocalValue));

        Assert.Equal(new DateTime(2026, 6, 2, 12, 30, 0, DateTimeKind.Utc), providerValue);
        Assert.Equal(DateTimeKind.Utc, providerValue.Kind);
    }

    [Fact]
    public void DateTimeConverter_LoadsUtcValuesUsingConfiguredOffset()
    {
        using var context = CreateContext(TimeSpan.FromHours(2));
        var converter = GetConverter<DateTime>(context, nameof(User.CreatedAt));
        var databaseValue = new DateTime(2026, 6, 2, 12, 30, 0, DateTimeKind.Utc);

        var modelValue = Assert.IsType<DateTime>(converter.ConvertFromProvider(databaseValue));

        Assert.Equal(new DateTime(2026, 6, 2, 14, 30, 0, DateTimeKind.Unspecified), modelValue);
        Assert.Equal(DateTimeKind.Unspecified, modelValue.Kind);
    }

    private static ValueConverter GetConverter<TProperty>(KnightsDbContext context, string propertyName)
    {
        var property = context.Model.FindEntityType(typeof(User))?.FindProperty(propertyName);

        Assert.NotNull(property);
        Assert.Equal(typeof(TProperty), property.ClrType);

        return Assert.IsAssignableFrom<ValueConverter>(property.GetValueConverter());
    }

    private static KnightsDbContext CreateContext(TimeSpan loadOffset)
    {
        var options = new DbContextOptionsBuilder<KnightsDbContext>()
            .UseInMemoryDatabase(Guid.NewGuid().ToString())
            .Options;

        return new KnightsDbContext(
            options,
            new FakeTenantContext(),
            Options.Create(new PersistenceDateTimeOptions { LoadOffset = loadOffset }));
    }

    private sealed class FakeTenantContext : ITenantContext
    {
        public Guid? TenantId => null;
    }
}
