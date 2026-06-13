using System.Text.Json;
using Knights.Application.Common.Interfaces;
using Knights.Application.Dashboards.Models;
using Knights.Domain.Dashboards;

namespace Knights.Application.Dashboards;

public sealed class DashboardService(IDashboardRepository repository, IUserContext userContext, IDataSourceCatalog catalog, IWidgetQueryExecutor queryExecutor) : IDashboardService
{
    private static readonly JsonSerializerOptions JsonOptions = new(JsonSerializerDefaults.Web);

    public async Task<IReadOnlyCollection<DashboardResponse>> GetAllAsync(CancellationToken cancellationToken = default) =>
        (await repository.GetOwnedAsync(userContext.UserId, cancellationToken)).Select(MapDashboard).ToArray();

    public async Task<DashboardResponse> CreateAsync(CreateDashboardRequest request, CancellationToken cancellationToken = default)
    {
        var dashboard = Dashboard.Create(userContext.TenantId, userContext.UserId, request.Name);
        await repository.AddAsync(dashboard, cancellationToken);
        return MapDashboard(dashboard);
    }

    public async Task<DashboardResponse> UpdateAsync(Guid id, UpdateDashboardRequest request, CancellationToken cancellationToken = default)
    {
        var dashboard = await GetRequiredDashboardAsync(id, cancellationToken);
        dashboard.Rename(request.Name);
        await repository.SaveAsync(cancellationToken);
        return MapDashboard(dashboard);
    }

    public async Task DeleteAsync(Guid id, CancellationToken cancellationToken = default)
    {
        var dashboard = await GetRequiredDashboardAsync(id, cancellationToken);
        await repository.DeleteAsync(dashboard, userContext.UserId.ToString(), cancellationToken);
    }

    public async Task<DashboardWidgetResponse> AddWidgetAsync(Guid dashboardId, SaveWidgetRequest request, CancellationToken cancellationToken = default)
    {
        ValidateQuery(request.DataSourceKey, request.Query);
        var dashboard = await GetRequiredDashboardAsync(dashboardId, cancellationToken);
        var widget = dashboard.AddWidget(ToCommand(request));
        await repository.SaveAsync(cancellationToken);
        return MapWidget(widget);
    }

    public async Task<DashboardWidgetResponse> UpdateWidgetAsync(Guid dashboardId, Guid widgetId, SaveWidgetRequest request, CancellationToken cancellationToken = default)
    {
        ValidateQuery(request.DataSourceKey, request.Query);
        var dashboard = await GetRequiredDashboardAsync(dashboardId, cancellationToken);
        var widget = GetRequiredWidget(dashboard, widgetId);
        widget.Update(ToCommand(request));
        await repository.SaveAsync(cancellationToken);
        return MapWidget(widget);
    }

    public async Task DeleteWidgetAsync(Guid dashboardId, Guid widgetId, CancellationToken cancellationToken = default)
    {
        var dashboard = await GetRequiredDashboardAsync(dashboardId, cancellationToken);
        GetRequiredWidget(dashboard, widgetId);
        dashboard.RemoveWidget(widgetId);
        await repository.SaveAsync(cancellationToken);
    }

    public async Task<WidgetDataResponse> GetWidgetDataAsync(Guid dashboardId, Guid widgetId, CancellationToken cancellationToken = default)
    {
        var dashboard = await GetRequiredDashboardAsync(dashboardId, cancellationToken);
        var widget = GetRequiredWidget(dashboard, widgetId);
        var query = Deserialize<WidgetQuerySpec>(widget.QuerySpecJson);
        ValidateQuery(widget.DataSourceKey, query);
        return await queryExecutor.ExecuteAsync(widget.DataSourceKey, query, cancellationToken);
    }

    private async Task<Dashboard> GetRequiredDashboardAsync(Guid id, CancellationToken cancellationToken) =>
        await repository.GetOwnedByIdAsync(id, userContext.UserId, cancellationToken) ?? throw new KeyNotFoundException($"Dashboard '{id}' was not found.");

    private void ValidateQuery(string sourceKey, WidgetQuerySpec query)
    {
        var source = catalog.GetRequired(sourceKey);
        var fields = source.Fields.ToDictionary(field => field.Key, StringComparer.OrdinalIgnoreCase);
        if (query.Limit is < 1 or > 100 || query.Fields.Count == 0 || query.Fields.Any(field => !fields.ContainsKey(field)))
            throw new ArgumentException("The widget query contains invalid fields or limits.");
        if (query.Filters?.Any(filter => !fields.TryGetValue(filter.Field, out var field) || !field.Filterable) == true)
            throw new ArgumentException("The widget query contains an invalid filter.");
        if (query.GroupBy is not null && (!fields.TryGetValue(query.GroupBy, out var groupField) || !groupField.Groupable))
            throw new ArgumentException("The widget query contains an invalid grouping field.");
        if (query.Aggregation is not null && !query.Aggregation.Function.Equals("count", StringComparison.OrdinalIgnoreCase))
            throw new ArgumentException($"Aggregation '{query.Aggregation.Function}' is not supported.");
        if (query.Aggregation is not null && (string.IsNullOrWhiteSpace(query.Aggregation.Alias) || query.Aggregation.Alias == query.GroupBy))
            throw new ArgumentException("The widget query contains an invalid aggregation alias.");
        if (query.Aggregation is not null && query.SortBy is not null && query.SortBy != query.Aggregation.Alias && query.SortBy != query.GroupBy)
            throw new ArgumentException("Aggregated widgets can only sort by the grouping field or aggregation alias.");
        if (query.Aggregation is null && query.SortBy is not null && !fields.ContainsKey(query.SortBy))
            throw new ArgumentException("The widget query contains an invalid sorting field.");
    }

    private static DashboardWidget GetRequiredWidget(Dashboard dashboard, Guid widgetId) =>
        dashboard.Widgets.FirstOrDefault(widget => widget.Id == widgetId) ?? throw new KeyNotFoundException($"Widget '{widgetId}' was not found.");

    private static AddDashboardWidget ToCommand(SaveWidgetRequest request) => new(request.Title, request.WidgetType, request.DataSourceKey, JsonSerializer.Serialize(request.Query, JsonOptions), JsonSerializer.Serialize(request.Visualization, JsonOptions), request.Row, request.Column, request.Width, request.Height);
    private static DashboardResponse MapDashboard(Dashboard dashboard) => new(dashboard.Id, dashboard.Name, dashboard.Slug, dashboard.Widgets.OrderBy(widget => widget.Row).ThenBy(widget => widget.Column).Select(MapWidget).ToArray());
    private static DashboardWidgetResponse MapWidget(DashboardWidget widget) => new(widget.Id, widget.Title, widget.WidgetType, widget.DataSourceKey, Deserialize<WidgetQuerySpec>(widget.QuerySpecJson), Deserialize<VisualizationConfig>(widget.VisualizationConfigJson), widget.Row, widget.Column, widget.Width, widget.Height);
    private static T Deserialize<T>(string json) => JsonSerializer.Deserialize<T>(json, JsonOptions) ?? throw new InvalidOperationException("Stored widget configuration is invalid.");
}
