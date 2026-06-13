using Knights.Application.Dashboards;
using Knights.Application.Dashboards.Models;
using Knights.Application.Common.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Knights.Api.Features.Dashboards;

[ApiController]
[Authorize]
[Route("api/dashboards")]
[Tags("Dashboards")]
public sealed class DashboardsController(
    IDashboardService dashboardService,
    IDataSourceCatalog catalog,
    IUserPermissionChecker permissionChecker) : ControllerBase
{
    [HttpGet]
    public async Task<IActionResult> GetAllAsync(CancellationToken cancellationToken) => Ok(await dashboardService.GetAllAsync(cancellationToken));

    [HttpPost]
    public async Task<IActionResult> CreateAsync(CreateDashboardRequest request, CancellationToken cancellationToken)
    {
        var dashboard = await dashboardService.CreateAsync(request, cancellationToken);
        return Created($"/api/dashboards/{dashboard.Id}", dashboard);
    }

    [HttpPut("{id:guid}")]
    public async Task<IActionResult> UpdateAsync(Guid id, UpdateDashboardRequest request, CancellationToken cancellationToken) => Ok(await dashboardService.UpdateAsync(id, request, cancellationToken));

    [HttpDelete("{id:guid}")]
    public async Task<IActionResult> DeleteAsync(Guid id, CancellationToken cancellationToken)
    {
        await dashboardService.DeleteAsync(id, cancellationToken);
        return NoContent();
    }

    [HttpPost("{dashboardId:guid}/widgets")]
    public async Task<IActionResult> AddWidgetAsync(Guid dashboardId, SaveWidgetRequest request, CancellationToken cancellationToken) => Ok(await dashboardService.AddWidgetAsync(dashboardId, request, cancellationToken));

    [HttpPut("{dashboardId:guid}/widgets/{widgetId:guid}")]
    public async Task<IActionResult> UpdateWidgetAsync(Guid dashboardId, Guid widgetId, SaveWidgetRequest request, CancellationToken cancellationToken) => Ok(await dashboardService.UpdateWidgetAsync(dashboardId, widgetId, request, cancellationToken));

    [HttpDelete("{dashboardId:guid}/widgets/{widgetId:guid}")]
    public async Task<IActionResult> DeleteWidgetAsync(Guid dashboardId, Guid widgetId, CancellationToken cancellationToken)
    {
        await dashboardService.DeleteWidgetAsync(dashboardId, widgetId, cancellationToken);
        return NoContent();
    }

    [HttpGet("{dashboardId:guid}/widgets/{widgetId:guid}/data")]
    public async Task<IActionResult> GetWidgetDataAsync(Guid dashboardId, Guid widgetId, CancellationToken cancellationToken) => Ok(await dashboardService.GetWidgetDataAsync(dashboardId, widgetId, cancellationToken));

    [HttpGet("datasources")]
    public async Task<IActionResult> GetDataSourcesAsync(CancellationToken cancellationToken)
    {
        var permittedSources = new List<DataSourceDescriptorResponse>();
        foreach (var source in catalog.GetAll())
        {
            if (await permissionChecker.HasPermissionAsync(source.RequiredPermission, cancellationToken))
                permittedSources.Add(source);
        }

        return Ok(permittedSources);
    }
}
