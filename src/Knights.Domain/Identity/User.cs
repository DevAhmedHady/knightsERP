using Knights.Domain.Common;
using Knights.Domain.Common.ValueObjects;

namespace Knights.Domain.Identity;

public class User : AuditedEntity
{
    private readonly List<UserRole> _roles = [];
    private readonly List<UserPermission> _permissions = [];

    private User()
    {
    }

    public NameValueObject Name { get; private set; } = null!;
    public string UserName { get; private set; } = string.Empty;
    public string Email { get; private set; } = string.Empty;
    public string? PasswordHash { get; private set; }
    public bool IsEmailConfirmed { get; private set; }
    public bool IsActive { get; private set; }
    public DateTime? LastLoginDate { get; private set; }
    public IReadOnlyCollection<UserRole> UserRoles => _roles.AsReadOnly();
    public IReadOnlyCollection<UserPermission> UserPermissions => _permissions.AsReadOnly();

    public static User Create(
        string firstName,
        string midName,
        string lastName,
        string userName,
        string email,
        string? passwordHash = null,
        bool isEmailConfirmed = false,
        Guid? id = null)
    {
        Validate(userName, email);

        return new User
        {
            Id = id.GetValueOrDefault(Guid.NewGuid()),
            Name = NameValueObject.Create(firstName, midName, lastName),
            UserName = userName.Trim(),
            Email = email.Trim(),
            PasswordHash = passwordHash,
            IsEmailConfirmed = isEmailConfirmed,
            IsActive = true
        };
    }

    public void UpdateName(string firstName, string midName, string lastName)
    {
        Name = NameValueObject.Create(firstName, midName, lastName);
    }

    public void UpdateProfile(string firstName, string midName, string lastName, string userName, string email, bool isEmailConfirmed)
    {
        Validate(userName, email);

        Name = NameValueObject.Create(firstName, midName, lastName);
        UserName = userName.Trim();
        Email = email.Trim();
        IsEmailConfirmed = isEmailConfirmed;
    }

    public void SetPasswordHash(string passwordHash)
    {
        ValidationRules.IsNotNullOrWhiteSpace(nameof(PasswordHash), passwordHash);
        PasswordHash = passwordHash;
    }

    public void SetActive(bool isActive)
    {
        IsActive = isActive;
    }

    public void ConfirmEmail()
    {
        IsEmailConfirmed = true;
    }

    public void RecordLogin(DateTime loginDate)
    {
        LastLoginDate = loginDate;
    }

    public UserRole AssignRole(Guid roleId)
    {
        ValidationRules.IsNotEmpty(nameof(roleId), roleId);

        var existing = _roles.FirstOrDefault(role => role.RoleId == roleId);
        if (existing is not null)
            return existing;

        var userRole = UserRole.Create(Id, roleId);
        _roles.Add(userRole);
        return userRole;
    }

    public void RemoveRole(Guid roleId)
    {
        ValidationRules.IsNotEmpty(nameof(roleId), roleId);
        _roles.RemoveAll(role => role.RoleId == roleId);
    }

    public UserPermission GrantPermission(Guid permissionId)
    {
        ValidationRules.IsNotEmpty(nameof(permissionId), permissionId);

        var existing = _permissions.FirstOrDefault(permission => permission.PermissionId == permissionId);
        if (existing is not null)
            return existing;

        var userPermission = UserPermission.Create(Id, permissionId);
        _permissions.Add(userPermission);
        return userPermission;
    }

    public void RevokePermission(Guid permissionId)
    {
        ValidationRules.IsNotEmpty(nameof(permissionId), permissionId);
        _permissions.RemoveAll(permission => permission.PermissionId == permissionId);
    }

    public override bool Equals(BaseEntity? other)
    {
        if (other is not User otherUser)
            return false;

        return Id == otherUser.Id &&
               Equals(Name, otherUser.Name) &&
               UserName == otherUser.UserName &&
               Email == otherUser.Email &&
               PasswordHash == otherUser.PasswordHash &&
               IsEmailConfirmed == otherUser.IsEmailConfirmed &&
               IsActive == otherUser.IsActive &&
               LastLoginDate == otherUser.LastLoginDate;
    }

    private static void Validate(string userName, string email)
    {
        ValidationRules.IsNotNullOrWhiteSpace(nameof(UserName), userName);
        ValidationRules.IsValidEmail(nameof(Email), email);
    }
}
