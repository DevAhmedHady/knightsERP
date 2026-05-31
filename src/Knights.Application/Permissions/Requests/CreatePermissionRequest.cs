namespace Knights.Application.Permissions.Requests;

public sealed record CreatePermissionRequest(
    string CodeName,
    string DisplayName,
    string Description);
