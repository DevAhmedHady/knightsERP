namespace Knights.Application.Tenants.Requests;

public sealed record UpdateFeatureCatalogItemRequest(
    string Name,
    string Description,
    string Category,
    IReadOnlyCollection<string> DependencyKeys,
    int DisplayOrder,
    bool IsPublished,
    bool IsRetired);
