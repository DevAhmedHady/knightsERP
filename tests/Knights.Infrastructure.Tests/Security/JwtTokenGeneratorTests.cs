using System.IdentityModel.Tokens.Jwt;
using Knights.Domain.Identity;
using Knights.Infrastructure.Security;
using Microsoft.Extensions.Options;

namespace Knights.Infrastructure.Tests.Security;

public sealed class JwtTokenGeneratorTests
{
    [Fact]
    public void Generate_ReturnsTokenWithFutureExpiration()
    {
        var generator = CreateGenerator();
        var user = CreateUser();

        var token = generator.Generate(user);

        Assert.False(string.IsNullOrWhiteSpace(token.Token));
        Assert.True(token.ExpiresAtUtc > DateTime.UtcNow);
    }

    [Fact]
    public void Generate_CreatesTokenWithExpectedIssuerAudienceAndClaims()
    {
        var generator = CreateGenerator();
        var user = CreateUser();

        var token = generator.Generate(user);
        var parsedToken = new JwtSecurityTokenHandler().ReadJwtToken(token.Token);

        Assert.Equal("i", parsedToken.Issuer);
        Assert.Contains("a", parsedToken.Audiences);
        Assert.Contains(parsedToken.Claims, claim => claim.Type == JwtRegisteredClaimNames.Sub && claim.Value == user.Id.ToString());
        Assert.Contains(parsedToken.Claims, claim => claim.Type == JwtRegisteredClaimNames.Email && claim.Value == user.Email);
    }

    private static JwtTokenGenerator CreateGenerator()
    {
        return new JwtTokenGenerator(Options.Create(new JwtOptions
        {
            Issuer = "i",
            Audience = "a",
            SecretKey = "a-test-secret-key-at-least-32-bytes-long!!",
            ExpiryMinutes = 60
        }));
    }

    private static User CreateUser()
    {
        return User.Create(
            firstName: "Ada",
            midName: "Byron",
            lastName: "Lovelace",
            userName: "ada.lovelace",
            email: "ada.lovelace@example.com",
            passwordHash: "hash",
            isEmailConfirmed: true,
            id: Guid.Parse("8f3ce115-fdc2-4d71-b632-fc6b563e53a0"));
    }
}
