using Domain.Domain.Base;
using System;
using System.Collections.Generic;
using System.Text;

namespace Core.Domain.Tenants
{
    public class Tenant : AuditedEntity
    {
        public string? CodeName { get; set; }
        public string? Name { get; set; }
        public string? Description { get; set; }
        public bool IsActive { get; set; }
        public DateTime? ExpiryDate { get; set; }
        public Guid OwenrId { get; set; }


        public static Tenant Create(string codeName, string name, string description, Guid ownerId, DateTime? expiryDate = null)
        {
            Validate(codeName, name, expiryDate);
            return new Tenant
            {
                CodeName = codeName.ToUpper().Replace(" ", "_"),
                Name = name,
                Description = description,
                IsActive = true,
                OwenrId = ownerId,
                ExpiryDate = expiryDate
            };
        }
        public void Update(string name, string description, DateTime? expiryDate)
        {
            Validate(CodeName!, name, expiryDate);
            Name = name;
            Description = description;
            ExpiryDate = expiryDate;
            // Code Name is not updated to preserve existing associations
        }

        public void SetActive(bool isActive)
        {
            IsActive = isActive;
        }

        private static void Validate(string codeName, string name, DateTime? expiryDate)
        {
            Consts.ValidationRoles.IsNotNullOrEmpty(codeName, nameof(codeName));
            Consts.ValidationRoles.IsNotNullOrEmpty(name, nameof(name));
            Consts.ValidationRoles.IsGreaterThan(nameof(ExpiryDate), expiryDate!.Value.Ticks, DateTime.Now.Ticks);
        }
    }
}
