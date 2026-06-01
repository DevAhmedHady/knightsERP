using Knights.Application.Tenants;
using Knights.Application.Tenants.Requests;

namespace Knights.Api.Endpoints;

public static class TenantEndpoints
{
    public static IEndpointRouteBuilder MapTenantEndpoints(this IEndpointRouteBuilder app)
    {
        var group = app.MapGroup("/api/tenants")
            .WithTags("Tenants");

        group.MapPost("", CreateAsync).WithName("CreateTenant");
        group.MapGet("/{id:guid}", GetByIdAsync).WithName("GetTenantById");
        group.MapGet("/code/{codeName}", GetByCodeNameAsync).WithName("GetTenantByCodeName");
        group.MapPut("/{id:guid}", UpdateAsync).WithName("UpdateTenant");
        group.MapPost("/{id:guid}/roles/{roleId:guid}", AssignRoleAsync).WithName("AssignTenantRole");
        group.MapDelete("/{id:guid}/roles/{roleId:guid}", RemoveRoleAsync).WithName("RemoveTenantRole");
        group.MapPost("/{id:guid}/permissions/{permissionId:guid}", GrantPermissionAsync).WithName("GrantTenantPermission");
        group.MapDelete("/{id:guid}/permissions/{permissionId:guid}", RevokePermissionAsync).WithName("RevokeTenantPermission");
        group.MapPost("/{id:guid}/members/{userId:guid}", AddMemberAsync).WithName("AddTenantMember");
        group.MapDelete("/{id:guid}/members/{userId:guid}", RemoveMemberAsync).WithName("RemoveTenantMember");

        return app;
    }

    private static async Task<IResult> CreateAsync(
        CreateTenantRequest request,
        ITenantService tenantService,
        CancellationToken cancellationToken)
    {
        var tenant = await tenantService.CreateAsync(request, cancellationToken);
        return Results.Created($"/api/tenants/{tenant.Id}", tenant);
    }

    private static async Task<IResult> GetByIdAsync(
        Guid id,
        ITenantService tenantService,
        CancellationToken cancellationToken)
    {
        var tenant = await tenantService.GetByIdAsync(id, cancellationToken);
        return tenant is null ? Results.NotFound() : Results.Ok(tenant);
    }

    private static async Task<IResult> GetByCodeNameAsync(
        string codeName,
        ITenantService tenantService,
        CancellationToken cancellationToken)
    {
        var tenant = await tenantService.GetByCodeNameAsync(codeName, cancellationToken);
        return tenant is null ? Results.NotFound() : Results.Ok(tenant);
    }

    private static async Task<IResult> UpdateAsync(
        Guid id,
        UpdateTenantRequest request,
        ITenantService tenantService,
        CancellationToken cancellationToken)
    {
        try
        {
            await tenantService.UpdateAsync(id, request, cancellationToken);
            return Results.NoContent();
        }
        catch (InvalidOperationException)
        {
            return Results.NotFound();
        }
    }

    private static async Task<IResult> AssignRoleAsync(
        Guid id,
        Guid roleId,
        ITenantService tenantService,
        CancellationToken cancellationToken)
    {
        try
        {
            await tenantService.AssignRoleAsync(id, roleId, cancellationToken);
            return Results.NoContent();
        }
        catch (InvalidOperationException)
        {
            return Results.NotFound();
        }
    }

    private static async Task<IResult> RemoveRoleAsync(
        Guid id,
        Guid roleId,
        ITenantService tenantService,
        CancellationToken cancellationToken)
    {
        try
        {
            await tenantService.RemoveRoleAsync(id, roleId, cancellationToken);
            return Results.NoContent();
        }
        catch (InvalidOperationException)
        {
            return Results.NotFound();
        }
    }

    private static async Task<IResult> GrantPermissionAsync(
        Guid id,
        Guid permissionId,
        ITenantService tenantService,
        CancellationToken cancellationToken)
    {
        try
        {
            await tenantService.GrantPermissionAsync(id, permissionId, cancellationToken);
            return Results.NoContent();
        }
        catch (InvalidOperationException)
        {
            return Results.NotFound();
        }
    }

    private static async Task<IResult> RevokePermissionAsync(
        Guid id,
        Guid permissionId,
        ITenantService tenantService,
        CancellationToken cancellationToken)
    {
        try
        {
            await tenantService.RevokePermissionAsync(id, permissionId, cancellationToken);
            return Results.NoContent();
        }
        catch (InvalidOperationException)
        {
            return Results.NotFound();
        }
    }

    private static async Task<IResult> AddMemberAsync(
        Guid id,
        Guid userId,
        ITenantService tenantService,
        CancellationToken cancellationToken)
    {
        try
        {
            await tenantService.AddMemberAsync(id, userId, cancellationToken);
            return Results.NoContent();
        }
        catch (InvalidOperationException)
        {
            return Results.NotFound();
        }
    }

    private static async Task<IResult> RemoveMemberAsync(
        Guid id,
        Guid userId,
        ITenantService tenantService,
        CancellationToken cancellationToken)
    {
        try
        {
            await tenantService.RemoveMemberAsync(id, userId, cancellationToken);
            return Results.NoContent();
        }
        catch (InvalidOperationException)
        {
            return Results.NotFound();
        }
    }
}
