using Core.Domain.Identity;
using Domain.Domain.Base;

namespace Core.Domain.Tenants
{
    public class TenantRole : AuditedEntity
    {
        public Guid TenantId { get; set; }
        public Guid RoleId { get; set; }

        public virtual Tenant? Tenant { get; set;  }
        public virtual Role? Role { get; set; }
    }
}
