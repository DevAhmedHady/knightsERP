using Knights.Domain.Common;

namespace Knights.Domain.Identity;

public class Role : AuditedEntity
{
    private readonly List<UserRole> _users = [];
    private readonly List<RolePermission> _permissions = [];

    private Role()
    {
    }

    public string CodeName { get; private set; } = string.Empty;
    public string Name { get; private set; } = string.Empty;
    public string Description { get; private set; } = string.Empty;
    public bool IsStatic { get; private set; }
    public bool IsDefault { get; private set; }
    public bool IsActive { get; private set; }
    public IReadOnlyCollection<UserRole> Users => _users.AsReadOnly();
    public IReadOnlyCollection<RolePermission> Permissions => _permissions.AsReadOnly();

    public static Role Create(string name, string description, bool isStatic, bool isDefault, bool isActive = true, Guid? id = null)
    {
        Validate(name, description);

        return new Role
        {
            Id = id.GetValueOrDefault(Guid.NewGuid()),
            CodeName = ToCodeName(name),
            Name = name.Trim(),
            Description = description.Trim(),
            IsActive = isActive,
            IsDefault = isDefault,
            IsStatic = isStatic
        };
    }

    public void Update(string name, string description)
    {
        Validate(name, description);

        Name = name.Trim();
        Description = description.Trim();
    }

    public void SetActive(bool isActive)
    {
        IsActive = isActive;
    }

    public void SetDefault(bool isDefault)
    {
        IsDefault = isDefault;
    }

    public void SetStatic(bool isStatic)
    {
        IsStatic = isStatic;
    }

    public void AssignPermission(Guid permissionId)
    {
        if (_permissions.Any(rp => rp.PermissionId == permissionId))
            return;

        _permissions.Add(RolePermission.Create(Id, permissionId));
    }

    public void RemovePermission(Guid permissionId)
    {
        var entry = _permissions.FirstOrDefault(rp => rp.PermissionId == permissionId);
        if (entry is not null)
            _permissions.Remove(entry);
    }

    public override bool Equals(BaseEntity? other)
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

    private static void Validate(string name, string description)
    {
        ValidationRules.IsNotNullOrWhiteSpace(nameof(Name), name);
        ValidationRules.IsNotNullOrWhiteSpace(nameof(Description), description);
    }

    private static string ToCodeName(string value)
    {
        return value.Trim().ToUpperInvariant().Replace(" ", "_");
    }
}
