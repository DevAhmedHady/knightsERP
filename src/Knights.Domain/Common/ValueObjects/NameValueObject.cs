namespace Knights.Domain.Common.ValueObjects;

public sealed class NameValueObject : ValueObject
{
    private NameValueObject(string firstName, string midName, string lastName)
    {
        FirstName = firstName;
        MidName = midName;
        LastName = lastName;
    }

    public string FirstName { get; }
    public string MidName { get; }
    public string LastName { get; }

    public static NameValueObject Create(string firstName, string midName, string lastName)
    {
        ValidationRules.IsNotNullOrWhiteSpace(nameof(FirstName), firstName);
        ValidationRules.IsNotNullOrWhiteSpace(nameof(MidName), midName);
        ValidationRules.IsNotNullOrWhiteSpace(nameof(LastName), lastName);

        return new NameValueObject(firstName.Trim(), midName.Trim(), lastName.Trim());
    }

    public override string ToString()
    {
        return string.Join(" ", new[] { FirstName, MidName, LastName }.Where(value => !string.IsNullOrWhiteSpace(value)));
    }

    protected override IEnumerable<object?> GetEqualityComponents()
    {
        yield return FirstName;
        yield return MidName;
        yield return LastName;
    }
}
