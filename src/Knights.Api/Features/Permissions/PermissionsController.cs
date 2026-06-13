using Knights.Application.Permissions;
using Knights.Application.Permissions.Requests;
using Microsoft.AspNetCore.Mvc;

namespace Knights.Api.Features.Permissions;

[ApiController]
[Route("api/permissions")]
[Tags("Permissions")]
public sealed class PermissionsController(IPermissionService permissionService) : ControllerBase
{
    [HttpPost(Name = "CreatePermission")]
    public async Task<IActionResult> CreatePermissionAsync(
        CreatePermissionRequest request,
        CancellationToken cancellationToken)
    {
        var permission = await permissionService.CreateAsync(request, cancellationToken);
        return Created($"/api/permissions/{permission.Id}", permission);
    }

    [HttpGet(Name = "GetAllPermissions")]
    public async Task<IActionResult> GetAllPermissionsAsync(CancellationToken cancellationToken)
    {
        var permissions = await permissionService.GetAllAsync(cancellationToken);
        return Ok(permissions);
    }

    [HttpGet("{id:guid}", Name = "GetPermissionById")]
    public async Task<IActionResult> GetPermissionByIdAsync(Guid id, CancellationToken cancellationToken)
    {
        var permission = await permissionService.GetByIdAsync(id, cancellationToken);
        return permission is null ? NotFound() : Ok(permission);
    }

    [HttpPut("{id:guid}", Name = "UpdatePermission")]
    public async Task<IActionResult> UpdatePermissionAsync(
        Guid id,
        UpdatePermissionRequest request,
        CancellationToken cancellationToken)
    {
        try
        {
            var permission = await permissionService.UpdateAsync(id, request, cancellationToken);
            return Ok(permission);
        }
        catch (InvalidOperationException)
        {
            return NotFound();
        }
    }

    [HttpDelete("{id:guid}", Name = "DeletePermission")]
    public async Task<IActionResult> DeletePermissionAsync(Guid id, CancellationToken cancellationToken)
    {
        await permissionService.DeleteAsync(id, cancellationToken);
        return NoContent();
    }
}
