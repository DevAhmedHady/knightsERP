using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;

namespace Knights.Infrastructure.Persistence;

/// <summary>
/// Design-time factory used by `dotnet ef` so migrations can be scaffolded without booting the API host.
/// The connection string here is only used at design time; the runtime string comes from configuration.
/// </summary>
public sealed class KnightsDbContextFactory : IDesignTimeDbContextFactory<KnightsDbContext>
{
    public KnightsDbContext CreateDbContext(string[] args)
    {
        var connectionString = Environment.GetEnvironmentVariable("KNIGHTS_DB_CONNECTION")
            ?? "Host=localhost;Port=5432;Database=knights;Username=postgres;Password=postgres";

        var options = new DbContextOptionsBuilder<KnightsDbContext>()
            .UseNpgsql(connectionString)
            .Options;

        return new KnightsDbContext(options);
    }
}
