using Knights.Domain.Common;

namespace Knights.Domain.Tenants;

public class FeatureCatalogItem : AuditedEntity
{
    private FeatureCatalogItem()
    {
    }

    public string Key { get; private set; } = string.Empty;
    public string Name { get; private set; } = string.Empty;
    public string Description { get; private set; } = string.Empty;
    public string Category { get; private set; } = string.Empty;
    public string DependencyKeysSerialized { get; private set; } = string.Empty;
    public int DisplayOrder { get; private set; }
    public bool IsPublished { get; private set; }
    public bool IsRetired { get; private set; }

    public IReadOnlyCollection<string> DependencyKeys => DependencyKeysSerialized
        .Split(';', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries)
        .Distinct(StringComparer.OrdinalIgnoreCase)
        .ToArray();

    public static FeatureCatalogItem Create(
        string key,
        string name,
        string description,
        string category,
        IEnumerable<string>? dependencyKeys,
        int displayOrder,
        bool isPublished,
        Guid? id = null)
    {
        Validate(key, name, displayOrder);

        return new FeatureCatalogItem
        {
            Id = id.GetValueOrDefault(Guid.NewGuid()),
            Key = NormalizeKey(key),
            Name = name.Trim(),
            Description = description.Trim(),
            Category = category.Trim(),
            DependencyKeysSerialized = SerializeDependencies(dependencyKeys),
            DisplayOrder = displayOrder,
            IsPublished = isPublished
        };
    }

    public void Update(
        string name,
        string description,
        string category,
        IEnumerable<string>? dependencyKeys,
        int displayOrder,
        bool isPublished,
        bool isRetired)
    {
        Validate(Key, name, displayOrder);

        Name = name.Trim();
        Description = description.Trim();
        Category = category.Trim();
        DependencyKeysSerialized = SerializeDependencies(dependencyKeys);
        DisplayOrder = displayOrder;
        IsPublished = isPublished;
        IsRetired = isRetired;
    }

    public override bool Equals(BaseEntity? other)
    {
        return other is FeatureCatalogItem item &&
               Id == item.Id &&
               Key == item.Key &&
               Name == item.Name &&
               Description == item.Description &&
               Category == item.Category &&
               DependencyKeysSerialized == item.DependencyKeysSerialized &&
               DisplayOrder == item.DisplayOrder &&
               IsPublished == item.IsPublished &&
               IsRetired == item.IsRetired;
    }

    private static void Validate(string key, string name, int displayOrder)
    {
        ValidationRules.IsNotNullOrWhiteSpace(nameof(Key), key);
        ValidationRules.IsNotNullOrWhiteSpace(nameof(Name), name);
        if (displayOrder < 0)
            throw new ArgumentOutOfRangeException(nameof(displayOrder), "Display order cannot be negative.");
    }

    private static string NormalizeKey(string key)
    {
        return key.Trim().ToUpperInvariant().Replace(" ", "_");
    }

    private static string SerializeDependencies(IEnumerable<string>? dependencyKeys)
    {
        return string.Join(";",
            (dependencyKeys ?? [])
                .Select(NormalizeKey)
                .Where(value => !string.IsNullOrWhiteSpace(value))
                .Distinct(StringComparer.OrdinalIgnoreCase));
    }
}
