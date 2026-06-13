using Knights.Application.Dashboards.Models;

namespace Knights.Application.Dashboards;

public interface IDashboardService
{
    Task<IReadOnlyCollection<DashboardResponse>> GetAllAsync(CancellationToken cancellationToken = default);
    Task<DashboardResponse> CreateAsync(CreateDashboardRequest request, CancellationToken cancellationToken = default);
    Task<DashboardResponse> UpdateAsync(Guid id, UpdateDashboardRequest request, CancellationToken cancellationToken = default);
    Task DeleteAsync(Guid id, CancellationToken cancellationToken = default);
    Task<DashboardWidgetResponse> AddWidgetAsync(Guid dashboardId, SaveWidgetRequest request, CancellationToken cancellationToken = default);
    Task<DashboardWidgetResponse> UpdateWidgetAsync(Guid dashboardId, Guid widgetId, SaveWidgetRequest request, CancellationToken cancellationToken = default);
    Task DeleteWidgetAsync(Guid dashboardId, Guid widgetId, CancellationToken cancellationToken = default);
    Task<WidgetDataResponse> GetWidgetDataAsync(Guid dashboardId, Guid widgetId, CancellationToken cancellationToken = default);
}
