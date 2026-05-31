using System.Reflection;
using System.Security.Cryptography;
using System.Text;

namespace Knights.Domain.Common;

public abstract class AuditedEntity : BaseEntity
{
    public DateTime CreatedAt { get; private set; }
    public DateTime? UpdatedAt { get; private set; }
    public string? CreatedBy { get; private set; }
    public string? UpdatedBy { get; private set; }
    public bool IsDeleted { get; private set; }
    public string? DeletedBy { get; private set; }
    public DateTime? DeletedAt { get; private set; }
    public string? ETag { get; private set; }

    public void MarkAsCreated(string? createdBy, string? props = null)
    {
        CreatedAt = DateTime.UtcNow;
        CreatedBy = createdBy;
        ETag = GenerateETag(props);
    }

    public void MarkAsModified(string? modifiedBy, string? props = null)
    {
        UpdatedAt = DateTime.UtcNow;
        UpdatedBy = modifiedBy;
        ETag = GenerateETag(props);
    }

    public void MarkAsDeleted(string? deletedBy)
    {
        IsDeleted = true;
        DeletedAt = DateTime.UtcNow;
        DeletedBy = deletedBy;
        ETag = GenerateETag();
    }

    public bool HasChanges(string? props = null)
    {
        return ETag != GenerateETag(props);
    }

    private string GetFormattedProps()
    {
        var values = GetType()
            .GetProperties(BindingFlags.Public | BindingFlags.Instance)
            .Select(property => property.GetValue(this)?.ToString());

        return string.Join("-", values);
    }

    private string GenerateETag(string? props = null)
    {
        props ??= GetFormattedProps();

        using var sha256 = SHA256.Create();
        var inputBytes = Encoding.UTF8.GetBytes($"{Id}-{CreatedAt}-{UpdatedAt}-{props}");
        var hashBytes = sha256.ComputeHash(inputBytes);
        return Convert.ToHexStringLower(hashBytes);
    }
}
