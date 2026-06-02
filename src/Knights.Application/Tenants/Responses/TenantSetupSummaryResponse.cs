namespace Knights.Application.Tenants.Responses;

public sealed record TenantSetupSummaryResponse(
    Guid TenantId,
    string TenantName,
    string EnvironmentDisplayName,
    string ThemeKey,
    string WorldDescription,
    int ProgressPercent,
    bool IsUnlocked,
    bool IsComplete,
    IReadOnlyCollection<TenantSetupStepResponse> Steps,
    IReadOnlyCollection<FeatureCatalogItemResponse> AvailableFeatures,
    IReadOnlyCollection<FeatureCatalogItemResponse> SelectedFeatures);
