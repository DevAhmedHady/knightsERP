using System.Text;

namespace Knights.Domain.Exceptions;

public class ValidationException : Exception
{
    public ValidationException()
        : base("One or more validation failures have occurred.")
    {
        Errors = new Dictionary<string, string[]>();
    }

    public ValidationException(string propertyName, string errorMessage)
        : base($"Validation failed for '{propertyName}': {errorMessage}")
    {
        Errors = new Dictionary<string, string[]>
        {
            { propertyName, [errorMessage] }
        };
    }

    public ValidationException(IDictionary<string, string[]> errors)
        : base("One or more validation failures have occurred.")
    {
        Errors = errors;
    }

    public IDictionary<string, string[]> Errors { get; }

    public override string Message
    {
        get
        {
            if (Errors.Count == 0)
                return base.Message;

            var builder = new StringBuilder(base.Message);
            builder.AppendLine();

            foreach (var error in Errors)
                builder.AppendLine($"  {error.Key}: {string.Join(", ", error.Value)}");

            return builder.ToString();
        }
    }
}
