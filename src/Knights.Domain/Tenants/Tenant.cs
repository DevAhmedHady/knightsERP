using Knights.Domain.Common;
using Knights.Domain.Identity;

namespace Knights.Domain.Tenants;

public class Tenant : AuditedEntity
{
    private readonly List<TenantRole> _roles = [];
    private readonly List<TenantPermission> _permissions = [];
    private readonly List<TenantFeatureSelection> _featureSelections = [];

    private Tenant()
    {
    }

    public string CodeName { get; private set; } = string.Empty;
    public string Name { get; private set; } = string.Empty;
    public string Description { get; private set; } = string.Empty;
    public string EnvironmentDisplayName { get; private set; } = string.Empty;
    public string ThemeKey { get; private set; } = string.Empty;
    public string WorldDescription { get; private set; } = string.Empty;
    public int SessionTimeoutMinutes { get; private set; }
    public bool IsActive { get; private set; }
    public DateTime? ExpiryDate { get; private set; }
    public DateTime? SetupStartedAt { get; private set; }
    public DateTime? SetupCompletedAt { get; private set; }
    public bool IsExpired => ExpiryDate.HasValue && ExpiryDate.Value < DateTime.UtcNow;
    public Guid OwnerId { get; private set; }
    public IReadOnlyCollection<TenantRole> TenantRoles => _roles.AsReadOnly();
    public IReadOnlyCollection<TenantPermission> TenantPermissions => _permissions.AsReadOnly();
    public IReadOnlyCollection<TenantFeatureSelection> TenantFeatureSelections => _featureSelections.AsReadOnly();

    public static Tenant Create(string codeName, string name, string description, Guid ownerId, DateTime? expiryDate = null, Guid? id = null)
    {
        Validate(codeName, name, ownerId, expiryDate);

        return new Tenant
        {
            Id = id.GetValueOrDefault(Guid.NewGuid()),
            CodeName = ToCodeName(codeName),
            Name = name.Trim(),
            Description = description.Trim(),
            IsActive = true,
            OwnerId = ownerId,
            ExpiryDate = expiryDate,
            SessionTimeoutMinutes = 60,
            SetupStartedAt = DateTime.UtcNow
        };
    }

    public void Update(string name, string description, DateTime? expiryDate, int? sessionTimeoutMinutes = null)
    {
        ValidationRules.IsNotNullOrWhiteSpace(nameof(Name), name);
        ValidationRules.IsFutureDate(nameof(ExpiryDate), expiryDate);

        Name = name.Trim();
        Description = description.Trim();
        ExpiryDate = expiryDate;
        if (sessionTimeoutMinutes.HasValue)
        {
            SetSessionTimeoutMinutes(sessionTimeoutMinutes.Value);
        }
    }

    public void SetSessionTimeoutMinutes(int sessionTimeoutMinutes)
    {
        ValidationRules.IsBetween(nameof(SessionTimeoutMinutes), sessionTimeoutMinutes, 1, 1440);
        SessionTimeoutMinutes = sessionTimeoutMinutes;
    }

    public void SetActive(bool isActive)
    {
        IsActive = isActive;
    }

    public void ConfigureEnvironment(string environmentDisplayName, string themeKey, string worldDescription)
    {
        ValidationRules.IsNotNullOrWhiteSpace(nameof(EnvironmentDisplayName), environmentDisplayName);
        ValidationRules.IsNotNullOrWhiteSpace(nameof(ThemeKey), themeKey);

        EnvironmentDisplayName = environmentDisplayName.Trim();
        ThemeKey = themeKey.Trim();
        WorldDescription = worldDescription.Trim();
        SetupStartedAt ??= DateTime.UtcNow;
    }

    public TenantRole AssignRole(Guid roleId)
    {
        ValidationRules.IsNotEmpty(nameof(roleId), roleId);

        var existing = _roles.FirstOrDefault(role => role.RoleId == roleId);
        if (existing is not null)
            return existing;

        var tenantRole = TenantRole.Create(Id, roleId);
        _roles.Add(tenantRole);
        return tenantRole;
    }

    public void RemoveRole(Guid roleId)
    {
        ValidationRules.IsNotEmpty(nameof(roleId), roleId);
        _roles.RemoveAll(role => role.RoleId == roleId);
    }

    public TenantPermission GrantPermission(Guid permissionId)
    {
        ValidationRules.IsNotEmpty(nameof(permissionId), permissionId);

        var existing = _permissions.FirstOrDefault(p => p.PermissionId == permissionId);
        if (existing is not null)
            return existing;

        var tenantPermission = TenantPermission.Create(Id, permissionId);
        _permissions.Add(tenantPermission);
        return tenantPermission;
    }

    public void RevokePermission(Guid permissionId)
    {
        ValidationRules.IsNotEmpty(nameof(permissionId), permissionId);
        _permissions.RemoveAll(p => p.PermissionId == permissionId);
    }

    public TenantFeatureSelection SelectFeature(Guid featureCatalogItemId)
    {
        ValidationRules.IsNotEmpty(nameof(featureCatalogItemId), featureCatalogItemId);

        var existing = _featureSelections.FirstOrDefault(selection => selection.FeatureCatalogItemId == featureCatalogItemId);
        if (existing is not null)
            return existing;

        var selection = TenantFeatureSelection.Create(Id, featureCatalogItemId);
        _featureSelections.Add(selection);
        SetupStartedAt ??= DateTime.UtcNow;
        return selection;
    }

    public void RemoveFeature(Guid featureCatalogItemId)
    {
        ValidationRules.IsNotEmpty(nameof(featureCatalogItemId), featureCatalogItemId);
        _featureSelections.RemoveAll(selection => selection.FeatureCatalogItemId == featureCatalogItemId);
    }

    public void SyncSetupCompletion(int progressPercent)
    {
        if (progressPercent >= 100)
        {
            SetupCompletedAt ??= DateTime.UtcNow;
            return;
        }

        SetupCompletedAt = null;
    }

    public override bool Equals(BaseEntity? other)
    {
        if (other is not Tenant otherTenant)
            return false;

        return Id == otherTenant.Id &&
               CodeName == otherTenant.CodeName &&
               Name == otherTenant.Name &&
               Description == otherTenant.Description &&
               EnvironmentDisplayName == otherTenant.EnvironmentDisplayName &&
               ThemeKey == otherTenant.ThemeKey &&
               WorldDescription == otherTenant.WorldDescription &&
               SessionTimeoutMinutes == otherTenant.SessionTimeoutMinutes &&
               IsActive == otherTenant.IsActive &&
               ExpiryDate == otherTenant.ExpiryDate &&
               OwnerId == otherTenant.OwnerId;
    }

    private static void Validate(string codeName, string name, Guid ownerId, DateTime? expiryDate)
    {
        ValidationRules.IsNotNullOrWhiteSpace(nameof(CodeName), codeName);
        ValidationRules.IsNotNullOrWhiteSpace(nameof(Name), name);
        ValidationRules.IsNotEmpty(nameof(OwnerId), ownerId);
        ValidationRules.IsFutureDate(nameof(ExpiryDate), expiryDate);
    }

    private static string ToCodeName(string value)
    {
        return value.Trim().ToUpperInvariant().Replace(" ", "_");
    }
}
