using Knights.Domain.Common;

namespace Knights.Domain.Tenants;

public class TenantFeatureSelection : AuditedEntity
{
    private TenantFeatureSelection()
    {
    }

    public Guid TenantId { get; private set; }
    public Guid FeatureCatalogItemId { get; private set; }
    public string SettingsJson { get; private set; } = "{}";
    public Tenant? Tenant { get; private set; }
    public FeatureCatalogItem? FeatureCatalogItem { get; private set; }

    public static TenantFeatureSelection Create(Guid tenantId, Guid featureCatalogItemId, string? settingsJson = null, Guid? id = null)
    {
        ValidationRules.IsNotEmpty(nameof(tenantId), tenantId);
        ValidationRules.IsNotEmpty(nameof(featureCatalogItemId), featureCatalogItemId);

        return new TenantFeatureSelection
        {
            Id = id.GetValueOrDefault(Guid.NewGuid()),
            TenantId = tenantId,
            FeatureCatalogItemId = featureCatalogItemId,
            SettingsJson = NormalizeJson(settingsJson)
        };
    }

    public void UpdateSettings(string? settingsJson)
    {
        SettingsJson = NormalizeJson(settingsJson);
    }

    public override bool Equals(BaseEntity? other)
    {
        return other is TenantFeatureSelection selection &&
               Id == selection.Id &&
               TenantId == selection.TenantId &&
               FeatureCatalogItemId == selection.FeatureCatalogItemId &&
               SettingsJson == selection.SettingsJson;
    }

    private static string NormalizeJson(string? json)
    {
        var trimmed = string.IsNullOrWhiteSpace(json) ? "{}" : json.Trim();
        using var _ = System.Text.Json.JsonDocument.Parse(trimmed);
        return trimmed;
    }
}
