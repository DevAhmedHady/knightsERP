using Domain.Domain.Base;

namespace Core.Domain.Identity
{
    public class UserRole : AuditedEntity
    {
        public Guid UserId { get; set; }
        public Guid RoleId { get; set; }
        public virtual User User { get; set; } = null!;
        public virtual Role Role { get; set; } = null!;


        public static UserRole Create(Guid userId, Guid roleId)
        {
            Validate(userId, roleId);
            return new UserRole
            {
                UserId = userId,
                RoleId = roleId
            };
        }

        private static void Validate(Guid userId, Guid roleId)
        {
            if (userId == Guid.Empty)
                throw new ArgumentException("UserId cannot be empty.", nameof(userId));
            if (roleId == Guid.Empty)
                throw new ArgumentException("RoleId cannot be empty.", nameof(roleId));
        }
    }
}
