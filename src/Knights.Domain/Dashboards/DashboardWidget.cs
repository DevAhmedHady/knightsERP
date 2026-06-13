using System.Text.Json;
using Knights.Domain.Common;

namespace Knights.Domain.Dashboards;

public sealed class DashboardWidget : AuditedEntity
{
    private DashboardWidget() { }

    public Guid DashboardId { get; private set; }
    public string Title { get; private set; } = string.Empty;
    public WidgetType WidgetType { get; private set; }
    public string DataSourceKey { get; private set; } = string.Empty;
    public string QuerySpecJson { get; private set; } = "{}";
    public string VisualizationConfigJson { get; private set; } = "{}";
    public int Row { get; private set; }
    public int Column { get; private set; }
    public int Width { get; private set; }
    public int Height { get; private set; }

    public static DashboardWidget Create(Guid dashboardId, AddDashboardWidget command, Guid? id = null)
    {
        Validate(command);
        return new DashboardWidget
        {
            Id = id.GetValueOrDefault(Guid.NewGuid()),
            DashboardId = dashboardId,
            Title = command.Title.Trim(),
            WidgetType = command.WidgetType,
            DataSourceKey = command.DataSourceKey.Trim().ToLowerInvariant(),
            QuerySpecJson = NormalizeJson(command.QuerySpecJson),
            VisualizationConfigJson = NormalizeJson(command.VisualizationConfigJson),
            Row = command.Row,
            Column = command.Column,
            Width = command.Width,
            Height = command.Height
        };
    }

    public void Update(AddDashboardWidget command)
    {
        Validate(command);
        Title = command.Title.Trim();
        WidgetType = command.WidgetType;
        DataSourceKey = command.DataSourceKey.Trim().ToLowerInvariant();
        QuerySpecJson = NormalizeJson(command.QuerySpecJson);
        VisualizationConfigJson = NormalizeJson(command.VisualizationConfigJson);
        Row = command.Row;
        Column = command.Column;
        Width = command.Width;
        Height = command.Height;
    }

    public override bool Equals(BaseEntity? other) => other is DashboardWidget widget && Id == widget.Id;

    private static void Validate(AddDashboardWidget command)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(command.Title);
        ArgumentException.ThrowIfNullOrWhiteSpace(command.DataSourceKey);
        if (command.Row < 0 || command.Column < 0 || command.Width is < 1 or > 12 || command.Height is < 1 or > 12)
            throw new ArgumentOutOfRangeException(nameof(command), "Widget layout is outside the supported grid.");
        NormalizeJson(command.QuerySpecJson);
        NormalizeJson(command.VisualizationConfigJson);
    }

    private static string NormalizeJson(string json)
    {
        var normalized = string.IsNullOrWhiteSpace(json) ? "{}" : json.Trim();
        using var _ = JsonDocument.Parse(normalized);
        return normalized;
    }
}
