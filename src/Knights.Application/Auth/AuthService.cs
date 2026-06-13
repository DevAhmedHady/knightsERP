using Mapster;
using Knights.Application.Auth.Requests;
using Knights.Application.Auth.Responses;
using Knights.Application.Common.Interfaces;
using Knights.Application.Common.Mapping;
using Knights.Application.Users.Responses;
using Knights.Domain.Identity;

namespace Knights.Application.Auth;

public sealed class AuthService(
    IUserRepository userRepository,
    IPasswordHasher passwordHasher,
    IJwtTokenGenerator tokenGenerator,
    ITenantRepository tenantRepository,
    IJwtSessionPolicy jwtSessionPolicy) : IAuthService
{
    static AuthService()
    {
        MapsterConfig.Register();
    }

    public async Task<LoginResponse?> LoginAsync(LoginRequest request, CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrWhiteSpace(request.UserNameOrEmail) || string.IsNullOrWhiteSpace(request.Password))
            return null;

        var user = await ResolveUserAsync(request.UserNameOrEmail, cancellationToken);

        if (user is null || !user.IsActive || string.IsNullOrEmpty(user.PasswordHash))
            return null;

        if (!passwordHasher.Verify(request.Password, user.PasswordHash))
            return null;

        Guid? tenantId = null;
        string? tenantCodeName = null;

        if (!user.IsSystemAdmin)
        {
            if (string.IsNullOrWhiteSpace(request.TenantCodeName))
                return null;

            var tenant = await tenantRepository.GetByCodeNameAsync(request.TenantCodeName.Trim(), cancellationToken);
            if (tenant is null || tenant.Id != user.TenantId || !tenant.IsActive || tenant.IsExpired)
                return null;

            tenantId = tenant.Id;
            tenantCodeName = tenant.CodeName;
        }

        user.RecordLogin(DateTime.UtcNow);
        await userRepository.UpdateAsync(user, cancellationToken);

        var expiryMinutes = await jwtSessionPolicy.ResolveEffectiveSessionTimeoutMinutesAsync(user, tenantId, cancellationToken);
        var token = tokenGenerator.Generate(user, tenantId, tenantCodeName, expiryMinutes);
        return new LoginResponse(token.Token, token.ExpiresAtUtc, user.Adapt<UserResponse>(), tenantId, tenantCodeName);
    }

    private async Task<User?> ResolveUserAsync(string userNameOrEmail, CancellationToken cancellationToken)
    {
        var identifier = userNameOrEmail.Trim();

        return identifier.Contains('@')
            ? await userRepository.GetByEmailAsync(identifier, cancellationToken)
              ?? await userRepository.GetByUserNameAsync(identifier, cancellationToken)
            : await userRepository.GetByUserNameAsync(identifier, cancellationToken)
              ?? await userRepository.GetByEmailAsync(identifier, cancellationToken);
    }
}
