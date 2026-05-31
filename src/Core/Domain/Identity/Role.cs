using Domain.Domain.Base;
using System;
using System.Collections.Generic;
using System.Text;

namespace Core.Domain.Identity
{
    public class Role : AuditedEntity
    {
        public string? CodeName { get; set;  }
        public string? Name { get; set; }
        public string? Description { get; set; }
        public bool IsStatic { get; set; } // Indicates if the role is static (cannot be deleted or modified)
        public bool IsDefault { get; set; } // Indicates if the role is assigned to new users by default
        public bool IsActive { get; set; } // Indicates if the role is active and can be assigned to users

        private List<UserRole> _users = [];
        public IReadOnlyCollection<UserRole> Users => _users.AsReadOnly();


        public static Role Create(string name, string desc, bool isStatic, bool isDefault, bool isActive = true)
        {
            return new Role
            {
                CodeName = name.ToUpper().Replace(" ", "_"),
                Name = name,
                Description = desc,
                IsActive = isActive,
                IsDefault = isDefault,
                IsStatic = isStatic
            };
        }

        public void Update(string name, string desc) {
            Name = name;
            Description = desc;
            // Code Name is not updated to preserve existing permissions and associations
        }


        public void SetActive(bool isActive)
        {
            IsActive = isActive;
        }

        public override bool Equals(BaseEntity other)
        {
            if (other is not Role otherRole)
                return false;
            return Id == otherRole.Id &&
                   CodeName == otherRole.CodeName &&
                   Name == otherRole.Name &&
                   Description == otherRole.Description &&
                   IsStatic == otherRole.IsStatic &&
                   IsDefault == otherRole.IsDefault &&
                   IsActive == otherRole.IsActive;
        }

        public void SetDefault(bool isDefault)
        {
            IsDefault = isDefault;
        }

        public void SetStatic(bool isStatic)
        {
            IsStatic = isStatic;
        }

        private void Validate(string name, string desc)
        {
            Consts.ValidationRoles.IsNotNullOrEmpty(nameof(Name), name);
            Consts.ValidationRoles.IsNotNullOrEmpty(nameof(Description), desc);
        }

    }
}
