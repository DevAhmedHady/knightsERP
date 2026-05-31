using Knights.Domain.Common;

namespace Knights.Domain.Identity;

public class RolePermission : AuditedEntity
{
    private RolePermission()
    {
    }

    public Guid RoleId { get; private set; }
    public Guid PermissionId { get; private set; }
    public Role? Role { get; private set; }
    public Permission? Permission { get; private set; }

    public static RolePermission Create(Guid roleId, Guid permissionId, Guid? id = null)
    {
        ValidationRules.IsNotEmpty(nameof(RoleId), roleId);
        ValidationRules.IsNotEmpty(nameof(PermissionId), permissionId);

        return new RolePermission
        {
            Id = id.GetValueOrDefault(Guid.NewGuid()),
            RoleId = roleId,
            PermissionId = permissionId
        };
    }

    public override bool Equals(BaseEntity? other)
    {
        return other is RolePermission rp &&
               Id == rp.Id &&
               RoleId == rp.RoleId &&
               PermissionId == rp.PermissionId;
    }
}
