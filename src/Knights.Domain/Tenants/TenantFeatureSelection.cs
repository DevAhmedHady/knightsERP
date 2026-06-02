using Knights.Domain.Common;

namespace Knights.Domain.Tenants;

public class TenantFeatureSelection : AuditedEntity
{
    private TenantFeatureSelection()
    {
    }

    public Guid TenantId { get; private set; }
    public Guid FeatureCatalogItemId { get; private set; }
    public Tenant? Tenant { get; private set; }
    public FeatureCatalogItem? FeatureCatalogItem { get; private set; }

    public static TenantFeatureSelection Create(Guid tenantId, Guid featureCatalogItemId, Guid? id = null)
    {
        ValidationRules.IsNotEmpty(nameof(tenantId), tenantId);
        ValidationRules.IsNotEmpty(nameof(featureCatalogItemId), featureCatalogItemId);

        return new TenantFeatureSelection
        {
            Id = id.GetValueOrDefault(Guid.NewGuid()),
            TenantId = tenantId,
            FeatureCatalogItemId = featureCatalogItemId
        };
    }

    public override bool Equals(BaseEntity? other)
    {
        return other is TenantFeatureSelection selection &&
               Id == selection.Id &&
               TenantId == selection.TenantId &&
               FeatureCatalogItemId == selection.FeatureCatalogItemId;
    }
}
