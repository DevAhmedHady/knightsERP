using Knights.Domain.Common;

namespace Knights.Domain.Identity;

public class UserRole : AuditedEntity
{
    private UserRole()
    {
    }

    public Guid UserId { get; private set; }
    public Guid RoleId { get; private set; }
    public User? User { get; private set; }
    public Role? Role { get; private set; }

    public static UserRole Create(Guid userId, Guid roleId, Guid? id = null)
    {
        ValidationRules.IsNotEmpty(nameof(UserId), userId);
        ValidationRules.IsNotEmpty(nameof(RoleId), roleId);

        return new UserRole
        {
            Id = id.GetValueOrDefault(Guid.NewGuid()),
            UserId = userId,
            RoleId = roleId
        };
    }

    public override bool Equals(BaseEntity? other)
    {
        return other is UserRole otherUserRole &&
               Id == otherUserRole.Id &&
               UserId == otherUserRole.UserId &&
               RoleId == otherUserRole.RoleId;
    }
}
