using Knights.Application.Common.Interfaces;

namespace Knights.Application.Tests.Fakes;

/// <summary>Deterministic test double: hash is "hashed:" + password.</summary>
public sealed class FakePasswordHasher : IPasswordHasher
{
    public string Hash(string password) => $"hashed:{password}";

    public bool Verify(string password, string hash) => hash == $"hashed:{password}";
}
