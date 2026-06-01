using Knights.Application.Users.Responses;

namespace Knights.Application.Auth.Responses;

public sealed record LoginResponse(string AccessToken, DateTime ExpiresAtUtc, UserResponse User, Guid? TenantId, string? TenantCodeName);
