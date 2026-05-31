namespace Knights.Application.Permissions.Responses;

public sealed record PermissionResponse(
    Guid Id,
    string CodeName,
    string DisplayName,
    string Description);
