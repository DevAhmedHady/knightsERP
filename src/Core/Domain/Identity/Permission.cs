using Domain.Domain.Base;

namespace Core.Domain.Identity
{
    public class Permission : AuditedEntity
    {
        public string? CodeName { get; set; }
        public string? DisplayName { get; set; }
        public string? Description { get; set; }

        private List<UserPermission> _users = [];

        public static Permission Create(string codeName, string displayName, string description)
        {
            return new Permission
            {
                CodeName = codeName.ToUpper().Replace(" ", "_"),
                DisplayName = displayName,
                Description = description
            };
        }

        public void Update(string displayName, string description)
        {
            DisplayName = displayName;
            Description = description;
            // Code Name is not updated to preserve existing associations
        }
        public override bool Equals(BaseEntity other)
        {
            if (other is not Permission otherPermission)
                return false;
            return Id == otherPermission.Id &&
                   CodeName == otherPermission.CodeName &&
                   DisplayName == otherPermission.DisplayName &&
                   Description == otherPermission.Description;
        }
        private void Validate(string codeName, string displayName)
        {
            Consts.ValidationRoles.IsNotNullOrEmpty(codeName, nameof(codeName));
            Consts.ValidationRoles.IsNotNullOrEmpty(displayName, nameof(displayName));
        }
    }
}