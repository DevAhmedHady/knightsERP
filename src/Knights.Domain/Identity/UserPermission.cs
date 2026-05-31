using Knights.Domain.Common;

namespace Knights.Domain.Identity;

public class UserPermission : AuditedEntity
{
    private UserPermission()
    {
    }

    public Guid UserId { get; private set; }
    public Guid PermissionId { get; private set; }
    public User? User { get; private set; }
    public Permission? Permission { get; private set; }

    public static UserPermission Create(Guid userId, Guid permissionId, Guid? id = null)
    {
        ValidationRules.IsNotEmpty(nameof(UserId), userId);
        ValidationRules.IsNotEmpty(nameof(PermissionId), permissionId);

        return new UserPermission
        {
            Id = id.GetValueOrDefault(Guid.NewGuid()),
            UserId = userId,
            PermissionId = permissionId
        };
    }

    public override bool Equals(BaseEntity? other)
    {
        return other is UserPermission otherUserPermission &&
               Id == otherUserPermission.Id &&
               UserId == otherUserPermission.UserId &&
               PermissionId == otherUserPermission.PermissionId;
    }
}
