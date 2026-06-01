using Knights.Infrastructure.Security;

namespace Knights.Infrastructure.Tests.Security;

public sealed class PasswordHasherTests
{
    private readonly PasswordHasher _hasher = new();

    [Fact]
    public void Verify_ReturnsTrue_ForCorrectPassword()
    {
        var hash = _hasher.Hash("correct-password");

        var verified = _hasher.Verify("correct-password", hash);

        Assert.True(verified);
    }

    [Fact]
    public void Verify_ReturnsFalse_ForWrongPassword()
    {
        var hash = _hasher.Hash("correct-password");

        var verified = _hasher.Verify("wrong-password", hash);

        Assert.False(verified);
    }

    [Fact]
    public void Hash_ProducesDifferentHashes_ForSamePassword_AndBothVerify()
    {
        const string password = "same-password";

        var firstHash = _hasher.Hash(password);
        var secondHash = _hasher.Hash(password);

        Assert.NotEqual(firstHash, secondHash);
        Assert.True(_hasher.Verify(password, firstHash));
        Assert.True(_hasher.Verify(password, secondHash));
    }

    [Theory]
    [InlineData("password", "")]
    [InlineData("password", "not-a-valid-hash")]
    [InlineData("password", "100000.not-base64.not-base64")]
    [InlineData("", "100000.c2FsdA==.aGFzaA==")]
    public void Verify_ReturnsFalse_ForMalformedOrEmptyHashAndEmptyPassword(string password, string hash)
    {
        var verified = _hasher.Verify(password, hash);

        Assert.False(verified);
    }
}
