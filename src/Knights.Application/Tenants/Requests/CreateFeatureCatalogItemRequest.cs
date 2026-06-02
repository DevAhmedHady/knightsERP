namespace Knights.Application.Tenants.Requests;

public sealed record CreateFeatureCatalogItemRequest(
    string Key,
    string Name,
    string Description,
    string Category,
    string IconKey,
    IReadOnlyCollection<string> Tags,
    IReadOnlyCollection<string> DependencyKeys,
    string SettingsSchemaJson,
    string DefaultSettingsJson,
    int SetupWeight,
    bool IsCore,
    int DisplayOrder,
    bool IsPublished);
