namespace Knights.Application.Roles.Requests;

public sealed record CreateRoleRequest(
    string Name,
    string Description,
    bool IsStatic = false,
    bool IsDefault = false,
    bool IsActive = true);
