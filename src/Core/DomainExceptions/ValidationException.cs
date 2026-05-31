using System.Text;

namespace Core.DomainExceptions
{
    public class ValidationException : Exception
    {
        public IDictionary<string, string[]> Errors { get; }

        public ValidationException()
            : base("One or more validation failures have occurred.")
        {
            Errors = new Dictionary<string, string[]>();
        }

        public ValidationException(string errorMsg)
            : base(errorMsg)
        {
            Errors = new Dictionary<string, string[]>
            {
                { "General", new[] { errorMsg } }
            };
        }

        public ValidationException(string propertyName, string errorMsg)
            : base($"Validation failed for '{propertyName}': {errorMsg}")
        {
            Errors = new Dictionary<string, string[]>
            {
                { propertyName, new[] { errorMsg } }
            };
        }

        public ValidationException(IDictionary<string, string[]> errors)
            : base("One or more validation failures have occurred.")
        {
            Errors = errors;
        }

        public ValidationException(IEnumerable<KeyValuePair<string, string>> errors)
            : base("One or more validation failures have occurred.")
        {
            Errors = errors
                .GroupBy(e => e.Key, e => e.Value)
                .ToDictionary(g => g.Key, g => g.ToArray());
        }

        public override string Message
        {
            get
            {
                if (Errors == null || !Errors.Any())
                    return base.Message;

                var builder = new StringBuilder(base.Message);
                builder.AppendLine();

                foreach (var error in Errors)
                {
                    builder.AppendLine($"  {error.Key}: {string.Join(", ", error.Value)}");
                }

                return builder.ToString();
            }
        }
    }
}
