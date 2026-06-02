namespace Knights.Application.Tenants.Responses;

public sealed record FeatureCatalogItemResponse(
    Guid Id,
    string Key,
    string Name,
    string Description,
    string Category,
    IReadOnlyCollection<string> DependencyKeys,
    int DisplayOrder,
    bool IsPublished,
    bool IsRetired);
