using Knights.Application.Auth;
using Knights.Application.Auth.Requests;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Knights.Api.Features.Auth;

[ApiController]
[Route("api/auth")]
[Tags("Auth")]
[AllowAnonymous]
public sealed class AuthController(IAuthService authService) : ControllerBase
{
    [HttpPost("login", Name = "Login")]
    public async Task<IActionResult> LoginAsync(
        LoginRequest request,
        CancellationToken cancellationToken)
    {
        var login = await authService.LoginAsync(request, cancellationToken);
        return login is null
            ? Unauthorized(new { title = "Invalid credentials." })
            : Ok(login);
    }
}
