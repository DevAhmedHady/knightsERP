using System.Reflection;
using System.Security.Cryptography;
using System.Text;

namespace Domain.Domain.Base
{
    public abstract class AuditedEntity : BaseEntity
    {
        public DateTime CreatedAt { get; set; }
        public DateTime? UpdatedAt { get; set; }
        public string? CreatedBy { get; set; }
        public string? UpdatedBy { get; set; }
        public bool IsDeleted { get; set; }
        public string? DeletedBy { get; set; }
        public DateTime? DeletedAt { get; set; }

        public string? ETag { get; private set; }


        public override bool Equals(BaseEntity other)
        {
            if (other is AuditedEntity auditedOther)
            {
                return Id == auditedOther.Id &&
                       CreatedAt == auditedOther.CreatedAt &&
                       UpdatedAt == auditedOther.UpdatedAt;
            }
            return false;
        }


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
        }

        private string GetFormattedProps()
        {
            var newProps = this.GetType()
                .GetProperties(BindingFlags.Public | BindingFlags.Instance)
                //.Where(p => p.IsDefined(typeof(ComparableAttribute), true))
                .Select(s => s.GetValue(this)?.ToString()).ToList();

            return String.Join("-", newProps);
        }

        private string GenerateETag(string? props = null)
        {
            props = props ?? GetFormattedProps();

            using SHA256 sha256 = SHA256.Create();
            byte[] inputBytes = Encoding.UTF8.GetBytes(String.Join("-", $"{Id}-{CreatedAt}-{UpdatedAt}", props));
            byte[] hashBytes = sha256.ComputeHash(inputBytes);
            return BitConverter.ToString(hashBytes).Replace("-", "").ToLower();
        }

        public bool HasChanges(string? props = null)
        {
            props = props ?? GetFormattedProps();
            string newEtag = GenerateETag(props);
            return ETag != newEtag;
        }
    }
}
