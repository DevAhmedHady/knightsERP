namespace Core.Domain.Base.ValueObjects
{
    public class NameValueObject : ValueObject
    {
        public string? FirstName { get; private set; }
        public string? MidName { get; private set; }
        public string? LastName { get; private set; }


        public NameValueObject Create(string firstName, string midName, string lastName)
        {
            var nameObj = new NameValueObject()
            {
                FirstName = firstName,
                MidName = midName,
                LastName = lastName
            };
            return nameObj;
        }

        public NameValueObject Create(string fullName)
        {
            var parts = fullName.Split(' ', StringSplitOptions.RemoveEmptyEntries);
            return new NameValueObject()
            {
                FirstName = parts.Length > 0 ? parts[0] : null,
                MidName = parts.Length > 2 ? string.Join(" ", parts, 1, parts.Length - 2) : null,
                LastName = parts.Length > 1 ? parts[^1] : null
            };
        }
        public NameValueObject Update(string firstName, string midName, string lastName)
        {
            var nameObj = new NameValueObject()
            {
                FirstName = firstName,
                MidName = midName,
                LastName = lastName
            };
            return nameObj;
        }


        public override string ToString()
        {
            var parts = new[] { FirstName, MidName, LastName };
            return string.Join(" ", parts.Where(p => !string.IsNullOrWhiteSpace(p)));
        }


        protected override IEnumerable<object> GetEqualityComponents()
        {
            yield return FirstName!;
            yield return MidName ?? "";
            yield return LastName!;
        }
    }
}
