using Knights.Application.Roles;
using Knights.Application.Roles.Requests;
using Microsoft.AspNetCore.Mvc;

namespace Knights.Api.Features.Roles;

[ApiController]
[Route("api/roles")]
[Tags("Roles")]
public sealed class RolesController(IRoleService roleService) : ControllerBase
{
    [HttpPost(Name = "CreateRole")]
    public async Task<IActionResult> CreateRoleAsync(
        CreateRoleRequest request,
        CancellationToken cancellationToken)
    {
        var role = await roleService.CreateAsync(request, cancellationToken);
        return Created($"/api/roles/{role.Id}", role);
    }

    [HttpGet(Name = "GetAllRoles")]
    public async Task<IActionResult> GetAllRolesAsync(CancellationToken cancellationToken)
    {
        var roles = await roleService.GetAllAsync(cancellationToken);
        return Ok(roles);
    }

    [HttpGet("{id:guid}", Name = "GetRoleById")]
    public async Task<IActionResult> GetRoleByIdAsync(Guid id, CancellationToken cancellationToken)
    {
        var role = await roleService.GetByIdAsync(id, cancellationToken);
        return role is null ? NotFound() : Ok(role);
    }

    [HttpPut("{id:guid}", Name = "UpdateRole")]
    public async Task<IActionResult> UpdateRoleAsync(
        Guid id,
        UpdateRoleRequest request,
        CancellationToken cancellationToken)
    {
        try
        {
            var role = await roleService.UpdateAsync(id, request, cancellationToken);
            return Ok(role);
        }
        catch (InvalidOperationException)
        {
            return NotFound();
        }
    }

    [HttpDelete("{id:guid}", Name = "DeleteRole")]
    public async Task<IActionResult> DeleteRoleAsync(Guid id, CancellationToken cancellationToken)
    {
        await roleService.DeleteAsync(id, cancellationToken);
        return NoContent();
    }

    [HttpPost("{id:guid}/permissions/{permissionId:guid}", Name = "AssignRolePermission")]
    public async Task<IActionResult> AssignPermissionAsync(
        Guid id,
        Guid permissionId,
        CancellationToken cancellationToken)
    {
        try
        {
            var role = await roleService.AssignPermissionAsync(id, permissionId, cancellationToken);
            return Ok(role);
        }
        catch (InvalidOperationException)
        {
            return NotFound();
        }
    }

    [HttpDelete("{id:guid}/permissions/{permissionId:guid}", Name = "RemoveRolePermission")]
    public async Task<IActionResult> RemovePermissionAsync(
        Guid id,
        Guid permissionId,
        CancellationToken cancellationToken)
    {
        try
        {
            var role = await roleService.RemovePermissionAsync(id, permissionId, cancellationToken);
            return Ok(role);
        }
        catch (InvalidOperationException)
        {
            return NotFound();
        }
    }
}
