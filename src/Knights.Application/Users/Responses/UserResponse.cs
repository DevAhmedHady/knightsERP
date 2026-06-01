namespace Knights.Application.Users.Responses;

public sealed record UserResponse(
    Guid Id,
    string FirstName,
    string MidName,
    string LastName,
    string UserName,
    string Email,
    bool IsEmailConfirmed,
    bool IsActive,
    DateTime? LastLoginDate,
    Guid? TenantId,
    IReadOnlyCollection<Guid> RoleIds,
    IReadOnlyCollection<Guid> PermissionIds);
