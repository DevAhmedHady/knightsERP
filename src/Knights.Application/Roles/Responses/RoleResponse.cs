namespace Knights.Application.Roles.Responses;

public sealed record RoleResponse(
    Guid Id,
    string CodeName,
    string Name,
    string Description,
    bool IsStatic,
    bool IsDefault,
    bool IsActive);
