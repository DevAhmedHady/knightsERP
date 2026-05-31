namespace Knights.Application.Permissions.Requests;

public sealed record UpdatePermissionRequest(
    string DisplayName,
    string Description);
