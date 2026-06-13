using Knights.Domain.Dashboards;

namespace Knights.Application.Dashboards.Models;

public sealed record DashboardResponse(Guid Id, string Name, string Slug, IReadOnlyCollection<DashboardWidgetResponse> Widgets);
public sealed record DashboardWidgetResponse(Guid Id, string Title, WidgetType WidgetType, string DataSourceKey, WidgetQuerySpec Query, VisualizationConfig Visualization, int Row, int Column, int Width, int Height);
public sealed record CreateDashboardRequest(string Name);
public sealed record UpdateDashboardRequest(string Name);
public sealed record SaveWidgetRequest(string Title, WidgetType WidgetType, string DataSourceKey, WidgetQuerySpec Query, VisualizationConfig Visualization, int Row, int Column, int Width, int Height);
public sealed record WidgetQuerySpec(IReadOnlyCollection<string> Fields, IReadOnlyCollection<WidgetFilter>? Filters = null, string? GroupBy = null, WidgetAggregation? Aggregation = null, string? SortBy = null, bool SortDescending = false, int Limit = 25);
public sealed record WidgetFilter(string Field, string Operator, string Value);
public sealed record WidgetAggregation(string Function, string Alias);
public sealed record VisualizationConfig(string ChartType = "bar", string? LabelField = null, string? ValueField = null);
public sealed record DataSourceDescriptorResponse(string Key, string Name, string RequiredPermission, IReadOnlyCollection<DataSourceFieldResponse> Fields);
public sealed record DataSourceFieldResponse(string Key, string Name, string DataType, bool Filterable, bool Groupable);
public sealed record WidgetDataResponse(IReadOnlyCollection<string> Columns, IReadOnlyCollection<IReadOnlyDictionary<string, object?>> Rows);
