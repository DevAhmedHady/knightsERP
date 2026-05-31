using Core.Domain.Base.ValueObjects;
using Core.Dto;
using Domain.Domain.Base;

namespace Core.Domain.Identity
{
    public class User : AuditedEntity
    {
        public NameValueObject? Name { get; set; }
        public string? UserName { get; set; }
        public string? Email { get; set; }
        public string? PasswordHash { get; set; }
        public bool IsEmailConfirmed { get; set; }
        public bool IsActive { get; set; }
        public DateTime? LastLoginDate { get; set; }

        private List<UserRole> _roles = [];
        public virtual IReadOnlyCollection<UserRole> UserRoles =>  _roles.AsReadOnly();

        private List<UserPermission> _permissions = [];
        public virtual IReadOnlyCollection<UserPermission> UserPermissions => _permissions.AsReadOnly();

        public override bool Equals(BaseEntity other)
        {
            if (other is not User otherUser)
                return false;
            return Id == otherUser.Id &&
                   EqualityComparer<NameValueObject?>.Default.Equals(Name, otherUser.Name) &&
                   UserName == otherUser.UserName &&
                   Email == otherUser.Email &&
                   PasswordHash == otherUser.PasswordHash &&
                   IsEmailConfirmed == otherUser.IsEmailConfirmed &&
                   IsActive == otherUser.IsActive &&
                   LastLoginDate == otherUser.LastLoginDate;
        }

        public static User Create(UserDto dto)
        {
            Validate(dto);
            var user = new User()
            {
                Id = dto.Id,
                Name = new NameValueObject().Create(dto.FirstName!, dto.MidName!, dto.LastName!),
                UserName = dto.UserName,
                Email = dto.Email,
                IsEmailConfirmed = dto.IsEmailConfirmed,
                IsActive = true
            };
            return user;
        }

        public User Update(UserDto dto)
        {
            Validate(dto);
            Name = new NameValueObject().Update(dto.FirstName!, dto.MidName!, dto.LastName!);
            UserName = dto.UserName;
            Email = dto.Email;
            IsEmailConfirmed = dto.IsEmailConfirmed;
            return this;
        }

        public void SetActive(bool isActive)
        {
            IsActive = isActive;
        }

        private static void Validate(UserDto user)
        {
            Consts.ValidationRoles.IsNotNullOrEmpty(nameof(UserName), user.UserName!);
            Consts.ValidationRoles.IsNotNullOrEmpty(nameof(Email), user.Email!);
            Consts.ValidationRoles.IsValidEmail(nameof(Email), user.Email!);
            Consts.ValidationRoles.IsNotNullOrEmpty(nameof(user.FirstName), user.FirstName!);
            Consts.ValidationRoles.IsNotNullOrEmpty(nameof(user.MidName), user.MidName!);
            Consts.ValidationRoles.IsNotNullOrEmpty(nameof(user.LastName), user.LastName!);
        }
    }
}
