namespace Knights.Application.Roles.Requests;

public sealed record UpdateRoleRequest(
    string Name,
    string Description);
