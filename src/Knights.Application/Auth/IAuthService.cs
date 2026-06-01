using Knights.Application.Auth.Requests;
using Knights.Application.Auth.Responses;

namespace Knights.Application.Auth;

public interface IAuthService
{
    /// <summary>
    /// Authenticates a user by username-or-email + password. Returns null on any failure
    /// (unknown user, wrong password, inactive account) without distinguishing the cause.
    /// </summary>
    Task<LoginResponse?> LoginAsync(LoginRequest request, CancellationToken cancellationToken = default);
}
