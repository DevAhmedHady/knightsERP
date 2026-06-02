namespace Knights.Application.Tenants.Requests;

public sealed record ConfigureTenantEnvironmentRequest(
    string EnvironmentDisplayName,
    string ThemeKey,
    string WorldDescription);
