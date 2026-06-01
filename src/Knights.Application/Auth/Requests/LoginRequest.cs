namespace Knights.Application.Auth.Requests;

public sealed record LoginRequest(string UserNameOrEmail, string Password, string? TenantCodeName = null);
