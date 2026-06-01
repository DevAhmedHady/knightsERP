using Knights.Application.Users;
using Knights.Application.Users.Requests;

namespace Knights.Api.Endpoints;

public static class UserEndpoints
{
    public static IEndpointRouteBuilder MapUserEndpoints(this IEndpointRouteBuilder app)
    {
        var group = app.MapGroup("/api/users")
            .WithTags("Users");

        group.MapPost("", CreateUserAsync)
            .WithName("CreateUser");

        group.MapGet("", GetAllUsersAsync)
            .WithName("GetAllUsers");

        group.MapGet("/{id:guid}", GetUserByIdAsync)
            .WithName("GetUserById");

        group.MapPut("/{id:guid}", UpdateUserAsync)
            .WithName("UpdateUser");

        group.MapPost("/{id:guid}/roles/{roleId:guid}", AssignRoleAsync)
            .WithName("AssignUserRole");

        group.MapPost("/{id:guid}/permissions/{permissionId:guid}", GrantPermissionAsync)
            .WithName("GrantUserPermission");

        return app;
    }

    private static async Task<IResult> CreateUserAsync(
        CreateUserRequest request,
        IUserService userService,
        CancellationToken cancellationToken)
    {
        var user = await userService.CreateAsync(request, cancellationToken);
        return Results.Created($"/api/users/{user.Id}", user);
    }

    private static async Task<IResult> GetAllUsersAsync(
        IUserService userService,
        CancellationToken cancellationToken)
    {
        var users = await userService.GetAllAsync(cancellationToken);
        return Results.Ok(users);
    }

    private static async Task<IResult> GetUserByIdAsync(
        Guid id,
        IUserService userService,
        CancellationToken cancellationToken)
    {
        var user = await userService.GetByIdAsync(id, cancellationToken);
        return user is null ? Results.NotFound() : Results.Ok(user);
    }

    private static async Task<IResult> UpdateUserAsync(
        Guid id,
        UpdateUserRequest request,
        IUserService userService,
        CancellationToken cancellationToken)
    {
        try
        {
            var user = await userService.UpdateAsync(id, request, cancellationToken);
            return Results.Ok(user);
        }
        catch (InvalidOperationException)
        {
            return Results.NotFound();
        }
    }

    private static async Task<IResult> AssignRoleAsync(
        Guid id,
        Guid roleId,
        IUserService userService,
        CancellationToken cancellationToken)
    {
        try
        {
            var user = await userService.AssignRoleAsync(id, roleId, cancellationToken);
            return Results.Ok(user);
        }
        catch (InvalidOperationException)
        {
            return Results.NotFound();
        }
    }

    private static async Task<IResult> GrantPermissionAsync(
        Guid id,
        Guid permissionId,
        IUserService userService,
        CancellationToken cancellationToken)
    {
        try
        {
            var user = await userService.GrantPermissionAsync(id, permissionId, cancellationToken);
            return Results.Ok(user);
        }
        catch (InvalidOperationException)
        {
            return Results.NotFound();
        }
    }
}
