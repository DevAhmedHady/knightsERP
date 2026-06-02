namespace Knights.Application.Tenants.Responses;

public sealed record TenantSelectedFeatureResponse(
    Guid Id,
    string Key,
    string Name,
    string Description,
    string Category,
    string IconKey,
    IReadOnlyCollection<string> Tags,
    IReadOnlyCollection<string> DependencyKeys,
    string SettingsSchemaJson,
    string DefaultSettingsJson,
    string SettingsJson,
    int SetupWeight,
    bool IsCore,
    int DisplayOrder,
    bool IsPublished,
    bool IsRetired);
