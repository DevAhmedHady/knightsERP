namespace Knights.Infrastructure.Persistence;

public sealed class PersistenceDateTimeOptions
{
    public const string SectionName = "Persistence:DateTime";

    public TimeSpan LoadOffset { get; set; } = TimeSpan.FromHours(2);
}
