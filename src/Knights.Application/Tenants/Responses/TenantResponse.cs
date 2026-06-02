namespace Knights.Application.Tenants.Responses;

public sealed record TenantResponse(
    Guid Id,
    string CodeName,
    string Name,
    string Description,
    string EnvironmentDisplayName,
    string ThemeKey,
    string WorldDescription,
    bool IsActive,
    DateTime? ExpiryDate,
    int SessionTimeoutMinutes,
    Guid OwnerId,
    DateTime? SetupStartedAt,
    DateTime? SetupCompletedAt,
    IReadOnlyCollection<Guid> RoleIds,
    IReadOnlyCollection<Guid> PermissionIds);
