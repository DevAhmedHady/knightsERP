using Knights.Application.Permissions;
using Knights.Application.Permissions.Requests;

namespace Knights.Api.Endpoints;

public static class PermissionEndpoints
{
    public static IEndpointRouteBuilder MapPermissionEndpoints(this IEndpointRouteBuilder app)
    {
        var group = app.MapGroup("/api/permissions")
            .WithTags("Permissions");

        group.MapPost("", CreatePermissionAsync).WithName("CreatePermission");
        group.MapGet("", GetAllPermissionsAsync).WithName("GetAllPermissions");
        group.MapGet("/{id:guid}", GetPermissionByIdAsync).WithName("GetPermissionById");
        group.MapPut("/{id:guid}", UpdatePermissionAsync).WithName("UpdatePermission");
        group.MapDelete("/{id:guid}", DeletePermissionAsync).WithName("DeletePermission");

        return app;
    }

    private static async Task<IResult> CreatePermissionAsync(
        CreatePermissionRequest request,
        IPermissionService permissionService,
        CancellationToken cancellationToken)
    {
        var permission = await permissionService.CreateAsync(request, cancellationToken);
        return Results.Created($"/api/permissions/{permission.Id}", permission);
    }

    private static async Task<IResult> GetAllPermissionsAsync(
        IPermissionService permissionService,
        CancellationToken cancellationToken)
    {
        var permissions = await permissionService.GetAllAsync(cancellationToken);
        return Results.Ok(permissions);
    }

    private static async Task<IResult> GetPermissionByIdAsync(
        Guid id,
        IPermissionService permissionService,
        CancellationToken cancellationToken)
    {
        var permission = await permissionService.GetByIdAsync(id, cancellationToken);
        return permission is null ? Results.NotFound() : Results.Ok(permission);
    }

    private static async Task<IResult> UpdatePermissionAsync(
        Guid id,
        UpdatePermissionRequest request,
        IPermissionService permissionService,
        CancellationToken cancellationToken)
    {
        try
        {
            var permission = await permissionService.UpdateAsync(id, request, cancellationToken);
            return Results.Ok(permission);
        }
        catch (InvalidOperationException)
        {
            return Results.NotFound();
        }
    }

    private static async Task<IResult> DeletePermissionAsync(
        Guid id,
        IPermissionService permissionService,
        CancellationToken cancellationToken)
    {
        await permissionService.DeleteAsync(id, cancellationToken);
        return Results.NoContent();
    }
}
