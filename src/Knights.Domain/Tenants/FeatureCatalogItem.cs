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
    public string IconKey { get; private set; } = string.Empty;
    public string TagsSerialized { get; private set; } = string.Empty;
    public string DependencyKeysSerialized { get; private set; } = string.Empty;
    public string SettingsSchemaJson { get; private set; } = "{}";
    public string DefaultSettingsJson { get; private set; } = "{}";
    public int SetupWeight { get; private set; }
    public bool IsCore { get; private set; }
    public int DisplayOrder { get; private set; }
    public bool IsPublished { get; private set; }
    public bool IsRetired { get; private set; }

    public IReadOnlyCollection<string> DependencyKeys => DependencyKeysSerialized
        .Split(';', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries)
        .Distinct(StringComparer.OrdinalIgnoreCase)
        .ToArray();
    public IReadOnlyCollection<string> Tags => TagsSerialized
        .Split(';', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries)
        .Distinct(StringComparer.OrdinalIgnoreCase)
        .ToArray();

    public static FeatureCatalogItem Create(
        string key,
        string name,
        string description,
        string category,
        string iconKey,
        IEnumerable<string>? tags,
        IEnumerable<string>? dependencyKeys,
        string settingsSchemaJson,
        string defaultSettingsJson,
        int setupWeight,
        bool isCore,
        int displayOrder,
        bool isPublished,
        Guid? id = null)
    {
        Validate(key, name, displayOrder, setupWeight, settingsSchemaJson, defaultSettingsJson);

        return new FeatureCatalogItem
        {
            Id = id.GetValueOrDefault(Guid.NewGuid()),
            Key = NormalizeKey(key),
            Name = name.Trim(),
            Description = description.Trim(),
            Category = category.Trim(),
            IconKey = iconKey.Trim(),
            TagsSerialized = SerializeValues(tags),
            DependencyKeysSerialized = SerializeDependencies(dependencyKeys),
            SettingsSchemaJson = NormalizeJson(settingsSchemaJson),
            DefaultSettingsJson = NormalizeJson(defaultSettingsJson),
            SetupWeight = setupWeight,
            IsCore = isCore,
            DisplayOrder = displayOrder,
            IsPublished = isPublished
        };
    }

    public void Update(
        string name,
        string description,
        string category,
        string iconKey,
        IEnumerable<string>? tags,
        IEnumerable<string>? dependencyKeys,
        string settingsSchemaJson,
        string defaultSettingsJson,
        int setupWeight,
        bool isCore,
        int displayOrder,
        bool isPublished,
        bool isRetired)
    {
        Validate(Key, name, displayOrder, setupWeight, settingsSchemaJson, defaultSettingsJson);

        Name = name.Trim();
        Description = description.Trim();
        Category = category.Trim();
        IconKey = iconKey.Trim();
        TagsSerialized = SerializeValues(tags);
        DependencyKeysSerialized = SerializeDependencies(dependencyKeys);
        SettingsSchemaJson = NormalizeJson(settingsSchemaJson);
        DefaultSettingsJson = NormalizeJson(defaultSettingsJson);
        SetupWeight = setupWeight;
        IsCore = isCore;
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
               IconKey == item.IconKey &&
               TagsSerialized == item.TagsSerialized &&
               DependencyKeysSerialized == item.DependencyKeysSerialized &&
               SettingsSchemaJson == item.SettingsSchemaJson &&
               DefaultSettingsJson == item.DefaultSettingsJson &&
               SetupWeight == item.SetupWeight &&
               IsCore == item.IsCore &&
               DisplayOrder == item.DisplayOrder &&
               IsPublished == item.IsPublished &&
               IsRetired == item.IsRetired;
    }

    private static void Validate(string key, string name, int displayOrder, int setupWeight, string settingsSchemaJson, string defaultSettingsJson)
    {
        ValidationRules.IsNotNullOrWhiteSpace(nameof(Key), key);
        ValidationRules.IsNotNullOrWhiteSpace(nameof(Name), name);
        if (displayOrder < 0)
            throw new ArgumentOutOfRangeException(nameof(displayOrder), "Display order cannot be negative.");
        if (setupWeight < 0 || setupWeight > 100)
            throw new ArgumentOutOfRangeException(nameof(setupWeight), "Setup weight must be between 0 and 100.");
        NormalizeJson(settingsSchemaJson);
        NormalizeJson(defaultSettingsJson);
    }

    private static string NormalizeKey(string key)
    {
        return key.Trim().ToUpperInvariant().Replace(" ", "_");
    }

    private static string SerializeDependencies(IEnumerable<string>? dependencyKeys)
    {
        return SerializeValues((dependencyKeys ?? []).Select(NormalizeKey));
    }

    private static string SerializeValues(IEnumerable<string>? values)
    {
        return string.Join(";",
            (values ?? [])
                .Select(value => value.Trim())
                .Where(value => !string.IsNullOrWhiteSpace(value))
                .Distinct(StringComparer.OrdinalIgnoreCase));
    }

    private static string NormalizeJson(string json)
    {
        var trimmed = string.IsNullOrWhiteSpace(json) ? "{}" : json.Trim();
        using var _ = System.Text.Json.JsonDocument.Parse(trimmed);
        return trimmed;
    }
}
