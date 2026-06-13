using Knights.Application.Users;
using Knights.Application.Users.Requests;
using Microsoft.AspNetCore.Mvc;

namespace Knights.Api.Features.Users;

[ApiController]
[Route("api/users")]
[Tags("Users")]
public sealed class UsersController(IUserService userService) : ControllerBase
{
    [HttpPost(Name = "CreateUser")]
    public async Task<IActionResult> CreateUserAsync(
        CreateUserRequest request,
        CancellationToken cancellationToken)
    {
        var user = await userService.CreateAsync(request, cancellationToken);
        return Created($"/api/users/{user.Id}", user);
    }

    [HttpGet(Name = "GetAllUsers")]
    public async Task<IActionResult> GetAllUsersAsync(CancellationToken cancellationToken)
    {
        var users = await userService.GetAllAsync(cancellationToken);
        return Ok(users);
    }

    [HttpGet("{id:guid}", Name = "GetUserById")]
    public async Task<IActionResult> GetUserByIdAsync(Guid id, CancellationToken cancellationToken)
    {
        var user = await userService.GetByIdAsync(id, cancellationToken);
        return user is null ? NotFound() : Ok(user);
    }

    [HttpPut("{id:guid}", Name = "UpdateUser")]
    public async Task<IActionResult> UpdateUserAsync(
        Guid id,
        UpdateUserRequest request,
        CancellationToken cancellationToken)
    {
        try
        {
            var user = await userService.UpdateAsync(id, request, cancellationToken);
            return Ok(user);
        }
        catch (InvalidOperationException)
        {
            return NotFound();
        }
    }

    [HttpDelete("{id:guid}", Name = "DeleteUser")]
    public async Task<IActionResult> DeleteUserAsync(Guid id, CancellationToken cancellationToken)
    {
        await userService.DeleteAsync(id, cancellationToken);
        return NoContent();
    }

    [HttpPost("{id:guid}/roles/{roleId:guid}", Name = "AssignUserRole")]
    public async Task<IActionResult> AssignRoleAsync(
        Guid id,
        Guid roleId,
        CancellationToken cancellationToken)
    {
        try
        {
            var user = await userService.AssignRoleAsync(id, roleId, cancellationToken);
            return Ok(user);
        }
        catch (InvalidOperationException)
        {
            return NotFound();
        }
    }

    [HttpPost("{id:guid}/permissions/{permissionId:guid}", Name = "GrantUserPermission")]
    public async Task<IActionResult> GrantPermissionAsync(
        Guid id,
        Guid permissionId,
        CancellationToken cancellationToken)
    {
        try
        {
            var user = await userService.GrantPermissionAsync(id, permissionId, cancellationToken);
            return Ok(user);
        }
        catch (InvalidOperationException)
        {
            return NotFound();
        }
    }
}
