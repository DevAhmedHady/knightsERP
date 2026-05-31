using System.Text.RegularExpressions;
using Knights.Domain.Exceptions;

namespace Knights.Domain.Common;

public static class ValidationRules
{
    public static void IsNotNullOrWhiteSpace(string propertyName, string? value)
    {
        if (string.IsNullOrWhiteSpace(value))
            throw new ValidationException(propertyName, $"{propertyName}: value cannot be null or empty.");
    }

    public static void IsNotEmpty(string propertyName, Guid value)
    {
        if (value == Guid.Empty)
            throw new ValidationException(propertyName, $"{propertyName}: value cannot be empty.");
    }

    public static void IsValidEmail(string propertyName, string? email)
    {
        const string pattern = @"^[a-zA-Z0-9.!#$%&'*+/=?^_`{|}~-]+@[a-zA-Z0-9-]+(?:\.[a-zA-Z0-9-]+)*$";

        if (string.IsNullOrWhiteSpace(email) || !Regex.IsMatch(email, pattern))
            throw new ValidationException(propertyName, $"{propertyName}: invalid email format.");
    }

    public static void IsFutureDate(string propertyName, DateTime? value)
    {
        if (value.HasValue && value.Value <= DateTime.UtcNow)
            throw new ValidationException(propertyName, $"{propertyName}: value must be in the future.");
    }
}
