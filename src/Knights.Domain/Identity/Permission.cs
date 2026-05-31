using Knights.Domain.Common;

namespace Knights.Domain.Identity;

public class Permission : AuditedEntity
{
    private readonly List<UserPermission> _users = [];

    private Permission()
    {
    }

    public string CodeName { get; private set; } = string.Empty;
    public string DisplayName { get; private set; } = string.Empty;
    public string Description { get; private set; } = string.Empty;
    public IReadOnlyCollection<UserPermission> Users => _users.AsReadOnly();

    public static Permission Create(string codeName, string displayName, string description, Guid? id = null)
    {
        Validate(codeName, displayName);

        return new Permission
        {
            Id = id.GetValueOrDefault(Guid.NewGuid()),
            CodeName = ToCodeName(codeName),
            DisplayName = displayName.Trim(),
            Description = description.Trim()
        };
    }

    public void Update(string displayName, string description)
    {
        ValidationRules.IsNotNullOrWhiteSpace(nameof(DisplayName), displayName);

        DisplayName = displayName.Trim();
        Description = description.Trim();
    }

    public override bool Equals(BaseEntity? other)
    {
        if (other is not Permission otherPermission)
            return false;

        return Id == otherPermission.Id &&
               CodeName == otherPermission.CodeName &&
               DisplayName == otherPermission.DisplayName &&
               Description == otherPermission.Description;
    }

    private static void Validate(string codeName, string displayName)
    {
        ValidationRules.IsNotNullOrWhiteSpace(nameof(CodeName), codeName);
        ValidationRules.IsNotNullOrWhiteSpace(nameof(DisplayName), displayName);
    }

    private static string ToCodeName(string value)
    {
        return value.Trim().ToUpperInvariant().Replace(" ", "_");
    }
}
