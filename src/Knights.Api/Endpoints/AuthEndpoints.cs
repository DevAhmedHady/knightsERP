using Knights.Application.Auth;
using Knights.Application.Auth.Requests;

namespace Knights.Api.Endpoints;

public static class AuthEndpoints
{
    public static IEndpointRouteBuilder MapAuthEndpoints(this IEndpointRouteBuilder app)
    {
        var group = app.MapGroup("/api/auth")
            .WithTags("Auth")
            .AllowAnonymous();

        group.MapPost("/login", LoginAsync)
            .WithName("Login");

        return app;
    }

    private static async Task<IResult> LoginAsync(
        LoginRequest request,
        IAuthService authService,
        CancellationToken cancellationToken)
    {
        var result = await authService.LoginAsync(request, cancellationToken);
        return result is null
            ? Results.Json(new { title = "Invalid credentials." }, statusCode: StatusCodes.Status401Unauthorized)
            : Results.Ok(result);
    }
}
