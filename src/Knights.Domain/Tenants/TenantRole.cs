using Knights.Domain.Common;
using Knights.Domain.Identity;

namespace Knights.Domain.Tenants;

public class TenantRole : AuditedEntity
{
    private TenantRole()
    {
    }

    public Guid TenantId { get; private set; }
    public Guid RoleId { get; private set; }
    public Tenant? Tenant { get; private set; }
    public Role? Role { get; private set; }

    public static TenantRole Create(Guid tenantId, Guid roleId, Guid? id = null)
    {
        ValidationRules.IsNotEmpty(nameof(TenantId), tenantId);
        ValidationRules.IsNotEmpty(nameof(RoleId), roleId);

        return new TenantRole
        {
            Id = id.GetValueOrDefault(Guid.NewGuid()),
            TenantId = tenantId,
            RoleId = roleId
        };
    }

    public override bool Equals(BaseEntity? other)
    {
        return other is TenantRole otherTenantRole &&
               Id == otherTenantRole.Id &&
               TenantId == otherTenantRole.TenantId &&
               RoleId == otherTenantRole.RoleId;
    }
}
