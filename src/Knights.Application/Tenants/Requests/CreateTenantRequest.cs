namespace Knights.Application.Tenants.Requests;

public sealed record CreateTenantRequest(string CodeName, string Name, string Description, Guid OwnerId, DateTime? ExpiryDate);
