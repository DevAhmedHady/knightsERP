using Knights.Application.Roles;
using Knights.Application.Roles.Requests;

namespace Knights.Api.Endpoints;

public static class RoleEndpoints
{
    public static IEndpointRouteBuilder MapRoleEndpoints(this IEndpointRouteBuilder app)
    {
        var group = app.MapGroup("/api/roles")
            .WithTags("Roles");

        group.MapPost("", CreateRoleAsync).WithName("CreateRole");
        group.MapGet("", GetAllRolesAsync).WithName("GetAllRoles");
        group.MapGet("/{id:guid}", GetRoleByIdAsync).WithName("GetRoleById");
        group.MapPut("/{id:guid}", UpdateRoleAsync).WithName("UpdateRole");
        group.MapDelete("/{id:guid}", DeleteRoleAsync).WithName("DeleteRole");
        group.MapPost("/{id:guid}/permissions/{permissionId:guid}", AssignPermissionAsync).WithName("AssignRolePermission");
        group.MapDelete("/{id:guid}/permissions/{permissionId:guid}", RemovePermissionAsync).WithName("RemoveRolePermission");

        return app;
    }

    private static async Task<IResult> CreateRoleAsync(
        CreateRoleRequest request,
        IRoleService roleService,
        CancellationToken cancellationToken)
    {
        var role = await roleService.CreateAsync(request, cancellationToken);
        return Results.Created($"/api/roles/{role.Id}", role);
    }

    private static async Task<IResult> GetAllRolesAsync(
        IRoleService roleService,
        CancellationToken cancellationToken)
    {
        var roles = await roleService.GetAllAsync(cancellationToken);
        return Results.Ok(roles);
    }

    private static async Task<IResult> GetRoleByIdAsync(
        Guid id,
        IRoleService roleService,
        CancellationToken cancellationToken)
    {
        var role = await roleService.GetByIdAsync(id, cancellationToken);
        return role is null ? Results.NotFound() : Results.Ok(role);
    }

    private static async Task<IResult> UpdateRoleAsync(
        Guid id,
        UpdateRoleRequest request,
        IRoleService roleService,
        CancellationToken cancellationToken)
    {
        try
        {
            var role = await roleService.UpdateAsync(id, request, cancellationToken);
            return Results.Ok(role);
        }
        catch (InvalidOperationException)
        {
            return Results.NotFound();
        }
    }

    private static async Task<IResult> DeleteRoleAsync(
        Guid id,
        IRoleService roleService,
        CancellationToken cancellationToken)
    {
        await roleService.DeleteAsync(id, cancellationToken);
        return Results.NoContent();
    }

    private static async Task<IResult> AssignPermissionAsync(
        Guid id,
        Guid permissionId,
        IRoleService roleService,
        CancellationToken cancellationToken)
    {
        try
        {
            var role = await roleService.AssignPermissionAsync(id, permissionId, cancellationToken);
            return Results.Ok(role);
        }
        catch (InvalidOperationException)
        {
            return Results.NotFound();
        }
    }

    private static async Task<IResult> RemovePermissionAsync(
        Guid id,
        Guid permissionId,
        IRoleService roleService,
        CancellationToken cancellationToken)
    {
        try
        {
            var role = await roleService.RemovePermissionAsync(id, permissionId, cancellationToken);
            return Results.Ok(role);
        }
        catch (InvalidOperationException)
        {
            return Results.NotFound();
        }
    }
}
