using Domain.Domain.Base;
using System;
using System.Collections.Generic;
using System.Text;

namespace Core.Domain.Identity
{
    public class UserPermission : AuditedEntity
    {
        public Guid UserId { get; set; }
        public Guid PermissionId { get; set; }

        public virtual User User { get; set; } = null!;
        public virtual Permission Permission { get; set; } = null!;
    }
}
