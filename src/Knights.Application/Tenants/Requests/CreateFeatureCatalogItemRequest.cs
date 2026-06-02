namespace Knights.Application.Tenants.Requests;

public sealed record CreateFeatureCatalogItemRequest(
    string Key,
    string Name,
    string Description,
    string Category,
    IReadOnlyCollection<string> DependencyKeys,
    int DisplayOrder,
    bool IsPublished);
