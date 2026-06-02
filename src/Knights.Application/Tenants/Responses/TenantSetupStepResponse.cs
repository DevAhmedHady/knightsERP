namespace Knights.Application.Tenants.Responses;

public sealed record TenantSetupStepResponse(
    string Code,
    string Title,
    bool IsCompleted,
    bool IsRequired);
