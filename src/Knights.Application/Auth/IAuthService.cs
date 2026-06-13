using Knights.Application.Auth.Requests;
using Knights.Application.Auth.Responses;

namespace Knights.Application.Auth;

public interface IAuthService
{
    Task<LoginResponse?> LoginAsync(LoginRequest request, CancellationToken cancellationToken = default);
}
