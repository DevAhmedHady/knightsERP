using Knights.Application.Dashboards.Models;

namespace Knights.Application.Dashboards;

public interface IWidgetQueryExecutor
{
    Task<WidgetDataResponse> ExecuteAsync(string dataSourceKey, WidgetQuerySpec query, CancellationToken cancellationToken = default);
}
