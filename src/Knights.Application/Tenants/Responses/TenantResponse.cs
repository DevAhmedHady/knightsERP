namespace Knights.Application.Tenants.Responses;

public sealed record TenantResponse(
    Guid Id,
    string CodeName,
    string Name,
    string Description,
    bool IsActive,
    DateTime? ExpiryDate,
    Guid OwnerId,
    IReadOnlyCollection<Guid> RoleIds,
    IReadOnlyCollection<Guid> PermissionIds);
