using Knights.Application.Tenants;
using Knights.Application.Tenants.Requests;
using Microsoft.AspNetCore.Mvc;

namespace Knights.Api.Features.Tenants;

[ApiController]
[Route("api/tenants")]
[Tags("Tenants")]
public sealed class TenantsController(ITenantService tenantService) : ControllerBase
{
    [HttpPost(Name = "CreateTenant")]
    public async Task<IActionResult> CreateAsync(
        CreateTenantRequest request,
        CancellationToken cancellationToken)
    {
        var tenant = await tenantService.CreateAsync(request, cancellationToken);
        return Created($"/api/tenants/{tenant.Id}", tenant);
    }

    [HttpGet(Name = "GetAllTenants")]
    public async Task<IActionResult> GetAllAsync(CancellationToken cancellationToken)
    {
        var tenants = await tenantService.GetAllAsync(cancellationToken);
        return Ok(tenants);
    }

    [HttpGet("{id:guid}", Name = "GetTenantById")]
    public async Task<IActionResult> GetByIdAsync(Guid id, CancellationToken cancellationToken)
    {
        var tenant = await tenantService.GetByIdAsync(id, cancellationToken);
        return tenant is null ? NotFound() : Ok(tenant);
    }

    [HttpGet("code/{codeName}", Name = "GetTenantByCodeName")]
    public async Task<IActionResult> GetByCodeNameAsync(
        string codeName,
        CancellationToken cancellationToken)
    {
        var tenant = await tenantService.GetByCodeNameAsync(codeName, cancellationToken);
        return tenant is null ? NotFound() : Ok(tenant);
    }

    [HttpPut("{id:guid}", Name = "UpdateTenant")]
    public async Task<IActionResult> UpdateAsync(
        Guid id,
        UpdateTenantRequest request,
        CancellationToken cancellationToken)
    {
        try
        {
            await tenantService.UpdateAsync(id, request, cancellationToken);
            return NoContent();
        }
        catch (InvalidOperationException)
        {
            return NotFound();
        }
    }

    [HttpPost("{id:guid}/deactivate", Name = "DeactivateTenant")]
    public async Task<IActionResult> DeactivateAsync(Guid id, CancellationToken cancellationToken)
    {
        try
        {
            await tenantService.DeactivateAsync(id, cancellationToken);
            return NoContent();
        }
        catch (InvalidOperationException)
        {
            return NotFound();
        }
    }

    [HttpPost("{id:guid}/roles/{roleId:guid}", Name = "AssignTenantRole")]
    public async Task<IActionResult> AssignRoleAsync(
        Guid id,
        Guid roleId,
        CancellationToken cancellationToken)
    {
        try
        {
            await tenantService.AssignRoleAsync(id, roleId, cancellationToken);
            return NoContent();
        }
        catch (InvalidOperationException)
        {
            return NotFound();
        }
    }

    [HttpDelete("{id:guid}/roles/{roleId:guid}", Name = "RemoveTenantRole")]
    public async Task<IActionResult> RemoveRoleAsync(
        Guid id,
        Guid roleId,
        CancellationToken cancellationToken)
    {
        try
        {
            await tenantService.RemoveRoleAsync(id, roleId, cancellationToken);
            return NoContent();
        }
        catch (InvalidOperationException)
        {
            return NotFound();
        }
    }

    [HttpPost("{id:guid}/permissions/{permissionId:guid}", Name = "GrantTenantPermission")]
    public async Task<IActionResult> GrantPermissionAsync(
        Guid id,
        Guid permissionId,
        CancellationToken cancellationToken)
    {
        try
        {
            await tenantService.GrantPermissionAsync(id, permissionId, cancellationToken);
            return NoContent();
        }
        catch (InvalidOperationException)
        {
            return NotFound();
        }
    }

    [HttpDelete("{id:guid}/permissions/{permissionId:guid}", Name = "RevokeTenantPermission")]
    public async Task<IActionResult> RevokePermissionAsync(
        Guid id,
        Guid permissionId,
        CancellationToken cancellationToken)
    {
        try
        {
            await tenantService.RevokePermissionAsync(id, permissionId, cancellationToken);
            return NoContent();
        }
        catch (InvalidOperationException)
        {
            return NotFound();
        }
    }

    [HttpPost("{id:guid}/members/{userId:guid}", Name = "AddTenantMember")]
    public async Task<IActionResult> AddMemberAsync(
        Guid id,
        Guid userId,
        CancellationToken cancellationToken)
    {
        try
        {
            await tenantService.AddMemberAsync(id, userId, cancellationToken);
            return NoContent();
        }
        catch (InvalidOperationException)
        {
            return NotFound();
        }
    }

    [HttpDelete("{id:guid}/members/{userId:guid}", Name = "RemoveTenantMember")]
    public async Task<IActionResult> RemoveMemberAsync(
        Guid id,
        Guid userId,
        CancellationToken cancellationToken)
    {
        try
        {
            await tenantService.RemoveMemberAsync(id, userId, cancellationToken);
            return NoContent();
        }
        catch (InvalidOperationException)
        {
            return NotFound();
        }
    }

    [HttpGet("current/setup", Name = "GetCurrentTenantSetup")]
    public async Task<IActionResult> GetCurrentSetupAsync(CancellationToken cancellationToken)
    {
        try
        {
            var summary = await tenantService.GetCurrentSetupAsync(cancellationToken);
            return Ok(summary);
        }
        catch (InvalidOperationException exception)
        {
            return BadRequest(new { error = exception.Message });
        }
    }

    [HttpPut("current/environment", Name = "ConfigureCurrentTenantEnvironment")]
    public async Task<IActionResult> ConfigureCurrentEnvironmentAsync(
        ConfigureTenantEnvironmentRequest request,
        CancellationToken cancellationToken)
    {
        try
        {
            var summary = await tenantService.ConfigureCurrentEnvironmentAsync(request, cancellationToken);
            return Ok(summary);
        }
        catch (InvalidOperationException exception)
        {
            return BadRequest(new { error = exception.Message });
        }
    }

    [HttpPost("current/features/{featureId:guid}", Name = "SelectCurrentTenantFeature")]
    public async Task<IActionResult> SelectCurrentFeatureAsync(
        Guid featureId,
        CancellationToken cancellationToken)
    {
        try
        {
            var summary = await tenantService.SelectCurrentFeatureAsync(featureId, cancellationToken);
            return Ok(summary);
        }
        catch (InvalidOperationException exception)
        {
            return BadRequest(new { error = exception.Message });
        }
    }

    [HttpDelete("current/features/{featureId:guid}", Name = "RemoveCurrentTenantFeature")]
    public async Task<IActionResult> RemoveCurrentFeatureAsync(
        Guid featureId,
        CancellationToken cancellationToken)
    {
        try
        {
            var summary = await tenantService.RemoveCurrentFeatureAsync(featureId, cancellationToken);
            return Ok(summary);
        }
        catch (InvalidOperationException exception)
        {
            return BadRequest(new { error = exception.Message });
        }
    }

    [HttpPut("current/features/{featureId:guid}/settings", Name = "UpdateCurrentTenantFeatureSettings")]
    public async Task<IActionResult> UpdateCurrentFeatureSettingsAsync(
        Guid featureId,
        UpdateTenantFeatureSettingsRequest request,
        CancellationToken cancellationToken)
    {
        try
        {
            var summary = await tenantService.UpdateCurrentFeatureSettingsAsync(featureId, request, cancellationToken);
            return Ok(summary);
        }
        catch (InvalidOperationException exception)
        {
            return BadRequest(new { error = exception.Message });
        }
    }

    [HttpGet("features/catalog", Name = "GetFeatureCatalog")]
    public async Task<IActionResult> GetFeatureCatalogAsync(CancellationToken cancellationToken)
    {
        var features = await tenantService.GetCatalogAsync(cancellationToken);
        return Ok(features);
    }

    [HttpPost("features/catalog", Name = "CreateFeatureCatalogItem")]
    public async Task<IActionResult> CreateFeatureCatalogAsync(
        CreateFeatureCatalogItemRequest request,
        CancellationToken cancellationToken)
    {
        try
        {
            var feature = await tenantService.CreateCatalogFeatureAsync(request, cancellationToken);
            return Created($"/api/tenants/features/catalog/{feature.Id}", feature);
        }
        catch (InvalidOperationException exception)
        {
            return BadRequest(new { error = exception.Message });
        }
    }

    [HttpPut("features/catalog/{featureId:guid}", Name = "UpdateFeatureCatalogItem")]
    public async Task<IActionResult> UpdateFeatureCatalogAsync(
        Guid featureId,
        UpdateFeatureCatalogItemRequest request,
        CancellationToken cancellationToken)
    {
        try
        {
            var feature = await tenantService.UpdateCatalogFeatureAsync(featureId, request, cancellationToken);
            return Ok(feature);
        }
        catch (InvalidOperationException exception)
        {
            return BadRequest(new { error = exception.Message });
        }
    }
}
