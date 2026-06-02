namespace Knights.Application.Tenants.Requests;

public sealed record UpdateTenantRequest(
    string Name,
    string Description,
    DateTime? ExpiryDate,
    int? SessionTimeoutMinutes = null);
