using Knights.Domain.Common;
using Knights.Domain.Identity;

namespace Knights.Domain.Tenants;

public class TenantPermission : AuditedEntity
{
    private TenantPermission()
    {
    }

    public Guid TenantId { get; private set; }
    public Guid PermissionId { get; private set; }
    public Tenant? Tenant { get; private set; }
    public Permission? Permission { get; private set; }

    public static TenantPermission Create(Guid tenantId, Guid permissionId, Guid? id = null)
    {
        ValidationRules.IsNotEmpty(nameof(TenantId), tenantId);
        ValidationRules.IsNotEmpty(nameof(PermissionId), permissionId);

        return new TenantPermission
        {
            Id = id.GetValueOrDefault(Guid.NewGuid()),
            TenantId = tenantId,
            PermissionId = permissionId
        };
    }

    public override bool Equals(BaseEntity? other)
    {
        return other is TenantPermission otherTenantPermission &&
               Id == otherTenantPermission.Id &&
               TenantId == otherTenantPermission.TenantId &&
               PermissionId == otherTenantPermission.PermissionId;
    }
}
