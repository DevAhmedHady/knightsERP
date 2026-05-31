namespace Knights.Application.Roles.Responses;

public sealed record RoleWithPermissionsResponse(
    Guid Id,
    string CodeName,
    string Name,
    string Description,
    bool IsStatic,
    bool IsDefault,
    bool IsActive,
    IReadOnlyCollection<Guid> PermissionIds);
