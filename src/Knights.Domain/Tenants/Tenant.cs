using Knights.Domain.Common;
using Knights.Domain.Identity;

namespace Knights.Domain.Tenants;

public class Tenant : AuditedEntity
{
    private readonly List<TenantRole> _roles = [];
    private readonly List<TenantPermission> _permissions = [];

    private Tenant()
    {
    }

    public string CodeName { get; private set; } = string.Empty;
    public string Name { get; private set; } = string.Empty;
    public string Description { get; private set; } = string.Empty;
    public bool IsActive { get; private set; }
    public DateTime? ExpiryDate { get; private set; }
    public bool IsExpired => ExpiryDate.HasValue && ExpiryDate.Value < DateTime.UtcNow;
    public Guid OwnerId { get; private set; }
    public IReadOnlyCollection<TenantRole> TenantRoles => _roles.AsReadOnly();
    public IReadOnlyCollection<TenantPermission> TenantPermissions => _permissions.AsReadOnly();

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
            ExpiryDate = expiryDate
        };
    }

    public void Update(string name, string description, DateTime? expiryDate)
    {
        ValidationRules.IsNotNullOrWhiteSpace(nameof(Name), name);
        ValidationRules.IsFutureDate(nameof(ExpiryDate), expiryDate);

        Name = name.Trim();
        Description = description.Trim();
        ExpiryDate = expiryDate;
    }

    public void SetActive(bool isActive)
    {
        IsActive = isActive;
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

    public override bool Equals(BaseEntity? other)
    {
        if (other is not Tenant otherTenant)
            return false;

        return Id == otherTenant.Id &&
               CodeName == otherTenant.CodeName &&
               Name == otherTenant.Name &&
               Description == otherTenant.Description &&
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
