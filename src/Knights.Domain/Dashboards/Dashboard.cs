using Knights.Domain.Common;

namespace Knights.Domain.Dashboards;

public sealed class Dashboard : AuditedEntity
{
    private readonly List<DashboardWidget> _widgets = [];

    private Dashboard() { }

    public Guid? TenantId { get; private set; }
    public Guid OwnerUserId { get; private set; }
    public string Name { get; private set; } = string.Empty;
    public string Slug { get; private set; } = string.Empty;
    public IReadOnlyCollection<DashboardWidget> Widgets => _widgets.AsReadOnly();

    public static Dashboard Create(Guid? tenantId, Guid ownerUserId, string name, Guid? id = null)
    {
        ArgumentOutOfRangeException.ThrowIfEqual(ownerUserId, Guid.Empty);
        ArgumentException.ThrowIfNullOrWhiteSpace(name);

        return new Dashboard
        {
            Id = id.GetValueOrDefault(Guid.NewGuid()),
            TenantId = tenantId,
            OwnerUserId = ownerUserId,
            Name = name.Trim(),
            Slug = CreateSlug(name)
        };
    }

    public void Rename(string name)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(name);
        Name = name.Trim();
        Slug = CreateSlug(name);
    }

    public DashboardWidget AddWidget(AddDashboardWidget command)
    {
        var widget = DashboardWidget.Create(Id, command);
        _widgets.Add(widget);
        return widget;
    }

    public void RemoveWidget(Guid widgetId)
    {
        _widgets.RemoveAll(widget => widget.Id == widgetId);
    }

    public override bool Equals(BaseEntity? other) => other is Dashboard dashboard && Id == dashboard.Id;

    private static string CreateSlug(string name)
    {
        var words = name.Trim().ToLowerInvariant().Split(' ', StringSplitOptions.RemoveEmptyEntries);
        return string.Join('-', words.Select(word => new string(word.Where(char.IsLetterOrDigit).ToArray())).Where(word => word.Length > 0));
    }
}

public sealed record AddDashboardWidget(
    string Title,
    WidgetType WidgetType,
    string DataSourceKey,
    string QuerySpecJson,
    string VisualizationConfigJson,
    int Row,
    int Column,
    int Width,
    int Height);
